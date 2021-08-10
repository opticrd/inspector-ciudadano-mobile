using Inspector.Framework.Dtos;
using Inspector.Framework.Dtos.Keycloak;
using Inspector.Framework.Dtos.Keycloak.Keycloak;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UIModule.Helpers.Rules;
using Xamarin.Essentials;
using Xamarin.Forms;
using Zammad.Client;


namespace Inspector.ViewModels
{
    public class SignupSocialMediaPageViewModel : BaseViewModel
    {
        IKeycloakApi _keycloakApi;
        IZammadLiteApi _zammadLiteApi;
        public ICommand GoogleCommand { get; set; }
        public ICommand FacebookCommand { get; set; }

        const string AuthenticationUrl = "https://citizens-auth-api-dev-i42qq4zxeq-ue.a.run.app/mobileauth/";

        private string _email;
        private string _document;
        public Citizen Citizen { get; set; }
        public string AuthToken { get; set; }
        public SignupSocialMediaPageViewModel(INavigationService navigationService, IPageDialogService dialogService,
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi)
            : base(navigationService, dialogService, cacheService)
        {
            _keycloakApi = keycloakApi;
            _zammadLiteApi = zammadLiteApi;

            GoogleCommand = new Command(async () => await OnAuthenticate("Google"));
            FacebookCommand = new Command(async () => await OnAuthenticate("Facebook"));
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                Citizen = parameters.GetValue<Citizen>("Citizen");
                _document = Citizen.Id;
            }
        }

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

                AuthToken = string.Empty;

                if (result.Properties.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name))
                    AuthToken += $"Name: {name}{Environment.NewLine}";
                if (result.Properties.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
                {
                    AuthToken += $"Email: {email}{Environment.NewLine}";
                    _email = email.Replace("%40", "@");

                    // Go to keycloak
                    var keycloakToken = await _keycloakApi.Authenticate(new TokenRequestBody
                    {
                        ClientId = "admin-cli",
                        GrantType = "password",
                        Password = "1234",
                        Username = "toribioea@gmail.com"
                    });
                    
                    var lastName = Citizen.FirstSurname ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(Citizen.SecondSurname))
                    {
                        lastName += " " + Citizen.SecondSurname;
                    }
                    var attributes = new Dictionary<string, List<string>>();
                    attributes.Add("cedula", new List<string>
                    {
                        Citizen.Id
                    });

                    // Create user with email and Password
                    var newKeycloakUser = new UserRepresentation
                    {
                        FirstName = Citizen.Names,
                        LastName = lastName,
                        EmailVerified = true,
                        Enabled = true,
                        Username = _email,
                        Email = _email,
                        Attributes = attributes
                    };
                    await _keycloakApi.CreateUser($"Bearer {keycloakToken.AccessToken}", newKeycloakUser);
                    var createdUser = await _keycloakApi.GetUser($"Bearer {keycloakToken.AccessToken}", _email);

                    // Login
                    await _keycloakApi.ResetPassword($"Bearer {keycloakToken.AccessToken}", createdUser[0].Id, new CredentialRepresentation
                    {
                        Password = Citizen.Id
                    });

                    await Login(_email, _document);
                }
                AuthToken += result?.AccessToken ?? result?.IdToken;

            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Login canceled.");

                AuthToken = string.Empty;
                await _dialogService.DisplayAlertAsync("", "Login canceled.", "Ok");
            }
            catch (Exception ex)
            {
                    Console.WriteLine($"Failed: {ex.Message}");

                AuthToken = string.Empty;
                await _dialogService.DisplayAlertAsync("", $"Failed: {ex.Message}", "Ok");
            }
            IsBusy = false;
        }


        private async Task Login(string email, string password)
        {
            try
            {

                //TODO Refactor this, pass these parameters with the appsettings file
                var keycloakToken = await _keycloakApi.Authenticate(new Framework.Dtos.Keycloak.TokenRequestBody
                {
                    ClientId = "admin-cli",
                    GrantType = "password",
                    Password = "1234",
                    Username = "toribioea@gmail.com"
                });
                var keycloakUserCollection = await _keycloakApi.GetUser($"Bearer {keycloakToken.AccessToken}", email);


                // Validate I get the user from keycloak
                if (keycloakUserCollection == null || keycloakUserCollection.Count != 1)
                    throw new System.Exception("El usuario no existe");
                var keycloakUser = keycloakUserCollection[0];

                // Get the cedula
                var cedula = keycloakUser.Attributes?.Cedula[0] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(cedula))
                    throw new System.Exception($"Tu usuario {email} en keycloak no tiene cédula. Contacta a un administrador.");

                // Search for the user email in zammad
                var zammadUserSearch = await _zammadLiteApi.SearchUser($"Bearer {AppKeys.ZammadToken}", email);

                // If the user doesn't exist in zammad, create it
                if (zammadUserSearch != null && zammadUserSearch.Where(x=>x.Email == email).Count() == 0)
                {
                    var zammadUser = await _zammadLiteApi.CreateUser($"Bearer {AppKeys.ZammadToken}", new ZammadUser
                    {
                        Email = email,
                        Firstname = keycloakUser.FirstName,
                        Lastname = keycloakUser.LastName,
                        Cedula = cedula,
                        Password = cedula,
                        Organization = "Ogtic",
                        Note = "Created from mobile",
                        Zone = "1",
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
        }
    }
}
