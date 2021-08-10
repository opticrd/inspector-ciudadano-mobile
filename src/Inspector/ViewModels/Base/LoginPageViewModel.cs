using Inspector.Framework.Dtos.Zammad;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Plugin.ValidationRules.Rules;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Zammad.Client;

namespace Inspector.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        IKeycloakApi _keycloakApi;
        IZammadLiteApi _zammadLiteApi;
        public LoginPageViewModel(INavigationService navigationService, IPageDialogService dialogService, 
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi) 
            : base(navigationService, dialogService, cacheService)
        {
            _keycloakApi = keycloakApi;
            _zammadLiteApi = zammadLiteApi;

            Email = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .IsEmail(Message.InvalidEmail);

            Password = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .Must(x => x.Length > 4, Message.MaxMinInvalidField);
            SignupCommand = new DelegateCommand(OnSignupCommandExecute);
            LoginCommand = new DelegateCommand(OnLoginCommandExecute);
            ForgetPasswordCommand = new DelegateCommand(()=> dialogService.DisplayAlertAsync(General.ForgetPassword, "Contacte su supervisor para mas información.", "Ok"));
        }

        private void OnSignupCommandExecute()
        {
            _navigationService.NavigateAsync("SignupDocumentPage");
        }

        public string VersionNumber
        {
            get
            {
                return VersionTracking.CurrentVersion;
            }
        }
        public Validatable<string> Password { get; set; }
        public Validatable<string> Email { get; set; }

        public ICommand LoginCommand { get; set; }
        public ICommand ForgetPasswordCommand { get; set; }
        public ICommand SignupCommand { get; set; }

        async void OnLoginCommandExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (!Email.Validate() || !Password.Validate())
            {
                IsBusy = false;
                return;
            }
                
            try
            {
                var email = Email.Value;
                var password = Password.Value;

                //TODO Refactor this, pass these parameters with the appsettings file
                var keycloakToken = await _keycloakApi.Authenticate(new Framework.Dtos.Keycloak.TokenRequestBody
                {
                    ClientId = "admin-cli",
                    GrantType = "password",
                    Password = "1234",
                    Username = "toribioea@gmail.com"
                });
                var keycloakUserCollection = await _keycloakApi.GetUser($"Bearer {keycloakToken.AccessToken}", Email.Value);


                // Validate I get the user from keycloak
                if(keycloakUserCollection == null || keycloakUserCollection.Count != 1)
                    throw new System.Exception("El usuario no existe");
                var keycloakUser = keycloakUserCollection[0];

                // Get the cedula
                var cedula = keycloakUser.Attributes?.Cedula[0]??string.Empty;
                if (string.IsNullOrWhiteSpace(cedula))
                    throw new System.Exception($"Tu usuario {Email.Value} en keycloak no tiene cédula. Contacta a un administrador.");

                // Search for the user email in zammad
                var zammadUserSearch = await _zammadLiteApi.SearchUser($"Bearer {AppKeys.ZammadToken}", Email.Value);

                // If the user doesn't exist in zammad, create it
                if (zammadUserSearch != null && zammadUserSearch.Where(x => x.Email == email).Count() == 0)
                {
                    var zammadUser = await _zammadLiteApi.CreateUser($"Bearer {AppKeys.ZammadToken}", new ZammadUser
                    {
                        Email = Email.Value,
                        Firstname = keycloakUser.FirstName,
                        Lastname = keycloakUser.LastName,
                        Cedula = cedula,
                        Password = cedula,
                        Organization = "Ogtic",
                        Note = "Created from mobile",
                        Verified = true,
                        //TODO: Revaluate this assignment
                        RoleIds = new List<int>() { 1 }, //1: Admin, 2: Agent, 3: Customer
                        Active = true
                    });
                }
                // TODO
                // If the user exists check if the user has the cedula field
                // If the cedula field is set, see if the password match the cedula
                // If the cedula field is not set, update the user, set the cedula field, and proceed




                var account = ZammadAccount.CreateBasicAccount(AppKeys.ZammadApiBaseUrl, email, password);
                var client = account.CreateUserClient();
                var userAccount = await client.GetUserMeAsync();

                if (userAccount.Active)
                {
                    var groupClient = account.CreateGroupClient();
                    var groups = await groupClient.GetGroupListAsync();

                    await _cacheService.InsertLocalObject(CacheKeys.Groups, groups);

                    Settings.IsLoggedIn = true;
                    await _cacheService.InsertSecureObject(CacheKeys.ZammadAccount, account);
                    await _cacheService.InsertSecureObject(CacheKeys.UserAccount, userAccount);

                    await _navigationService.NavigateAsync($"/{NavigationKeys.HomePage}");
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("", Message.AccountNotActivated, "Ok");
                }                    
            }
            catch (System.Exception ex)
            {
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
            }
            
            IsBusy = false;
        }
    }
}
