using Inspector.Framework.Dtos;
using Inspector.Framework.Dtos.Keycloak;
using Inspector.Framework.Dtos.Keycloak.Keycloak;
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
        public ICommand MicrosoftCommand { get; set; }

        const string AuthenticationUrl = "https://citizens-auth-api-dev-i42qq4zxeq-ue.a.run.app/mobileauth/";

        private string _email;
        private string _document;
        private string _password;
        private string _zone;
        private ZammadGroup _group;
        private List<ZammadGroup> _groups;

        public string FullName 
        {
            get
            {
                if (Citizen == null) return string.Empty;
                return $"{Citizen?.Names} {Citizen?.FirstSurname} {Citizen?.SecondSurname}";
            }
        }
        public string Location { get; set;  }
        public Citizen Citizen { get; set; }

        public SignupSocialMediaPageViewModel(INavigationService navigationService, IPageDialogService dialogService,
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi)
            : base(navigationService, dialogService, cacheService)
        {
            _keycloakApi = keycloakApi;
            _zammadLiteApi = zammadLiteApi;

            GoogleCommand = new Command(async () => await OnAuthenticate("Google"));
            FacebookCommand = new Command(async () => await OnAuthenticate("Facebook"));
            MicrosoftCommand = new Command(async () => await OnAuthenticate("Microsoft"));
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                Citizen = parameters.GetValue<Citizen>("Citizen");
                _document = Citizen.Id;
                _password = parameters.GetValue<string>("Password");
                _zone = parameters.GetValue<string>("ZoneCode");
                _group = parameters.GetValue<ZammadGroup>("Group");
                _groups = parameters.GetValue<List<ZammadGroup>>("Groups");

                var locationParts = new string[]
                {
                    parameters.GetValue<string>("Region"),
                    parameters.GetValue<string>("Province"),
                    parameters.GetValue<string>("Municipality"),
                    parameters.GetValue<string>("District"),
                };
                Location = string.Join(", ", locationParts.Where(x=>!string.IsNullOrEmpty(x)));
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
                    result = await WebAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions
                    {
                        PrefersEphemeralWebBrowserSession = true,
                        Url = authUrl,
                        CallbackUrl = callbackUrl,
                    });
                }

                if (result.Properties.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
                {
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
                    attributes.Add("pwd", new List<string>
                    {
                        _password.Base64Encode()
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

                    //If the user already exists, do a login
                    var keycloakUserCollection = await _keycloakApi.GetUser($"Bearer {keycloakToken.AccessToken}", _email);
                    if(keycloakUserCollection != null && keycloakUserCollection.Count == 1)
                    {
                        //Resets the password because we are doing a "registration"
                        await _keycloakApi.ResetPassword($"Bearer {keycloakToken.AccessToken}", keycloakUserCollection[0].Id, new CredentialRepresentation
                        {
                            Password = _password
                        });

                        await _keycloakApi.UpdateUser($"Bearer {keycloakToken.AccessToken}", keycloakUserCollection[0].Id, newKeycloakUser);

                        await Login(_email, _password);
                        IsBusy = false;
                        return;
                    }

                    await _keycloakApi.CreateUser($"Bearer {keycloakToken.AccessToken}", newKeycloakUser);
                    var createdUser = await _keycloakApi.GetUser($"Bearer {keycloakToken.AccessToken}", _email);

                    // Login
                    await _keycloakApi.ResetPassword($"Bearer {keycloakToken.AccessToken}", createdUser[0].Id, new CredentialRepresentation
                    {
                        Password = _password
                    });

                    await Login(_email, _password);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("", "No pudimos tomar tu correo del proveedor de identidad. Asegúrate de que tu correo sea público.", "Ok");
                }

            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Autenticación canceledada.");

                await _dialogService.DisplayAlertAsync("", "Autenticación canceledada.", "Ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed: {ex.Message}");

                await _dialogService.DisplayAlertAsync("", $"Ocurrió un error: {ex.Message}", "Ok");
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
                {
                    throw new System.Exception("El usuario no existe");
                }
                var keycloakUser = keycloakUserCollection[0];

                // Get the cedula
                var cedula = keycloakUser.Attributes?.Cedula[0] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(cedula))
                    throw new System.Exception($"Tu usuario {email} en keycloak no tiene cédula. Contacta a un administrador.");

                // Search for the user email in zammad
                var zammadUserSearch = await _zammadLiteApi.SearchUser($"Bearer {AppKeys.ZammadToken}", email);

                var zammadGroupsMap = new Dictionary<string, List<string>>();
                
                foreach(var group in _groups)
                {
                    zammadGroupsMap.Add(group.Id, new List<string> { "read", "create", "change" });
                }

                zammadGroupsMap[_group.Id].Clear();
                zammadGroupsMap[_group.Id].Add("full");

                // If the user doesn't exist in zammad, create it
                if (zammadUserSearch != null && zammadUserSearch.Where(x=>x.Email == email).Count() == 0)
                {
                    var zammadUser = await _zammadLiteApi.CreateUser($"Bearer {AppKeys.ZammadToken}", new ZammadUser
                    {
                        Email = email,
                        Firstname = keycloakUser.FirstName,
                        Lastname = keycloakUser.LastName,
                        Cedula = cedula,
                        Password = _password,
                        Organization = "Ogtic",
                        Note = "User created from mobile",
                        Zone = _zone,
                        Verified = true,
                        //TODO: Revaluate this assignment
                        RoleIds = new List<int>() { 2 }, //1: Admin, 2: Agent, 3: Customer
                        GroupIds = zammadGroupsMap,
                        Active = true
                    });
                }
                else
                {
                    var zammadUser = zammadUserSearch[0];
                    zammadUser.Password = password;
                    zammadUser.Zone = _zone;
                    zammadUser.Verified = true;
                    zammadUser.GroupIds = zammadGroupsMap;
                    await _zammadLiteApi.UpdateUser($"Bearer {AppKeys.ZammadToken}", zammadUser.Id, zammadUser);
                    //Update Password to document
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
