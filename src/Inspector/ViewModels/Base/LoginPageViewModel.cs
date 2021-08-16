using Inspector.Framework.Dtos.Keycloak;
using Inspector.Framework.Dtos.Zammad;
using Inspector.Framework.Helpers.Extensions;
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
using System;
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
        public ICommand GoogleCommand { get; set; }
        public ICommand FacebookCommand { get; set; }
        public ICommand MicrosoftCommand { get; set; }

        const string AuthenticationUrl = "https://citizens-auth-api-dev-i42qq4zxeq-ue.a.run.app/mobileauth/";
        public LoginPageViewModel(INavigationService navigationService, IPageDialogService dialogService, 
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi) 
            : base(navigationService, dialogService, cacheService)
        {
            _keycloakApi = keycloakApi;
            _zammadLiteApi = zammadLiteApi;

           /* Email = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .IsEmail(Message.InvalidEmail);

            Password = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .Must(x => x.Length > 4, Message.MaxMinInvalidField);*/

           // SignupCommand = new DelegateCommand(OnSignupCommandExecute);
            //LoginCommand = new DelegateCommand(OnLoginCommandExecute);

            GoogleCommand = new DelegateCommand(async () => await OnAuthenticate("Google"));
            FacebookCommand = new DelegateCommand(async () => await OnAuthenticate("Facebook"));
            MicrosoftCommand = new DelegateCommand(async () => await OnAuthenticate("Microsoft"));

            // ForgetPasswordCommand = new DelegateCommand(()=> dialogService.DisplayAlertAsync(General.ForgetPassword, "Contacte su supervisor para más información.", "Ok"));
        }

        private void OnSignupCommandExecute()
        {
            _navigationService.NavigateAsync("SignupDocumentPage");
        }

        //public Validatable<string> Password { get; set; }
        //public Validatable<string> Email { get; set; }

       // public ICommand LoginCommand { get; set; }
        //public ICommand ForgetPasswordCommand { get; set; }
        //public ICommand SignupCommand { get; set; }
        private async Task OnAuthenticate(string scheme)
        {
            try
            {
                IsBusy = true;
                WebAuthenticatorResult result = null;

                if (scheme.Equals("Apple") && DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version.Major >= 13)
                {
                    // Make sure to enable Apple Sign In in both the
                    // entitlements and the provisioning profile.
                    var options = new AppleSignInAuthenticator.Options
                    {
                        IncludeEmailScope = true,
                        IncludeFullNameScope = true,
                    };
                    result = await AppleSignInAuthenticator.AuthenticateAsync(options);
                }
                else
                {
                    var authUrl = new Uri(AuthenticationUrl + scheme);
                    var callbackUrl = new Uri("ogticapp://");

                    result = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl);
                }

                if (result.Properties.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
                {
                    email = email.Replace("%40", "@");

                    // Go to keycloak
                    var keycloakToken = await _keycloakApi.Authenticate(new TokenRequestBody
                    {
                        ClientId = "admin-cli",
                        GrantType = "password",
                        Password = "1234",
                        Username = "toribioea@gmail.com"
                    });

                    //If the user already exists, do a login
                    var keycloakUserCollection = await _keycloakApi.GetUser($"Bearer {keycloakToken.AccessToken}", email);
                    if (keycloakUserCollection != null && keycloakUserCollection.Count == 1)
                    {
                        var pwd = keycloakUserCollection[0]?.Attributes?.Pwd[0] ?? string.Empty;
                        await DoLogin(email, pwd.Base64Decode());
                        IsBusy = false;
                        return;
                    }

                }

            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Login canceled.");

                await _dialogService.DisplayAlertAsync("", "Login cancelado.", "Ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed: {ex.Message}");

                await _dialogService.DisplayAlertAsync("", $"Failed: {ex.Message}", "Ok");
            }
            IsBusy = false;
        }
        /*
        async void OnLoginCommandExecute()
        {
            if (IsBusy)
                return;

            if (!Email.Validate() || !Password.Validate())
            {
                return;
            }
            await DoLogin(Email.Value, Password.Value);
        }*/
        async Task DoLogin(string email, string password) 
        { 
            IsBusy = true;

            try
            {
                //TODO Refactor this, pass these parameters with the appsettings file
               /* var keycloakToken = await _keycloakApi.Authenticate(new Framework.Dtos.Keycloak.TokenRequestBody
                {
                    ClientId = "admin-cli",
                    GrantType = "password",
                    Password = "1234",
                    Username = "toribioea@gmail.com"
                });
                var keycloakUserCollection = await _keycloakApi.GetUser($"Bearer {keycloakToken.AccessToken}", email);


                // Validate I get the user from keycloak
                if (keycloakUserCollection == null || keycloakUserCollection.Count != 1)
                {
                    throw new System.Exception("El usuario no existe");
                }

                var keycloakUser = keycloakUserCollection[0];

                // Get the cedula
                /*var cedula = keycloakUser.Attributes?.Cedula[0]??string.Empty;
                if (string.IsNullOrWhiteSpace(cedula))
                    throw new System.Exception($"Tu usuario {email} en keycloak no tiene cédula. Contacta a un administrador.");

                // Search for the user email in zammad
                var zammadUserSearch = await _zammadLiteApi.SearchUser($"Bearer {AppKeys.ZammadToken}", email);

                // If the user doesn't exist in zammad, create it
                if (zammadUserSearch != null && zammadUserSearch.Where(x => x.Email == email).Count() == 0)
                {
                    var zammadUser = await _zammadLiteApi.CreateUser($"Bearer {AppKeys.ZammadToken}", new ZammadUser
                    {
                        Email = email,
                        Firstname = keycloakUser.FirstName,
                        Lastname = keycloakUser.LastName,
                        Cedula = cedula,
                        Password = password,
                        Organization = "Ogtic",
                        Note = "Created from mobile",
                        Verified = true,
                        //TODO: Revaluate this assignment
                        RoleIds = new List<int>() { 2 }, //1: Admin, 2: Agent, 3: Customer
                        Active = true
                    });
                }*/
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
