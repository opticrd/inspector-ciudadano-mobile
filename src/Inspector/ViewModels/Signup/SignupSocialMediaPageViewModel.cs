using Inspector.Framework.Dtos;
using Inspector.Framework.Dtos.Zammad;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Microsoft.AppCenter.Crashes;
using Prism.Logging;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class SignupSocialMediaPageViewModel : BaseViewModel
    {
        IKeycloakApi _keycloakApi;
        IZammadLiteApi _zammadLiteApi;
        IAuthService _authService;
        public ICommand GoogleCommand { get; set; }
        public ICommand FacebookCommand { get; set; }
        public ICommand MicrosoftCommand { get; set; }

        const string AuthenticationUrl = "https://citizens-auth-api-dev-i42qq4zxeq-ue.a.run.app/mobileauth/";

        private string _email;
        private string _document;
        private string _password;
        private string _zone;
        //private Organization _organization;
        //private List<Organization> _organizations;
        ILogger _logger;

        public string FullName 
        {
            get
            {
                if (Citizen == null) return string.Empty;
                return $"{Citizen?.Names} {Citizen?.FirstSurname} {Citizen?.SecondSurname}";
            }
        }
        public string Location { get; set;  }
        public Citizen Citizen { get; set; } = new Citizen();
        public Organization Organization { get; set; } = new Organization();

        public SignupSocialMediaPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ILogger logger, IAuthService authService,
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi)
            : base(navigationService, dialogService, cacheService)
        {
            _logger = logger;
            _authService = authService;
            _keycloakApi = keycloakApi;
            _zammadLiteApi = zammadLiteApi;

            GoogleCommand = new Command(async () => await OnAuthenticate("Google"));
            FacebookCommand = new Command(async () => await OnAuthenticate("Facebook"));
            MicrosoftCommand = new Command(async () => await OnAuthenticate("Microsoft"));
        }

        private async Task OnAuthenticate(string scheme)
        {
            IMaterialModalPage loadingDialog = null;

            try
            {
                using (loadingDialog = await MaterialDialog.Instance.LoadingDialogAsync(message: "Por favor, espere..."))
                {
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

                        var lastName = Citizen.FirstSurname ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(Citizen.SecondSurname))
                        {
                            lastName += " " + Citizen.SecondSurname;
                        }

                        // Create user with email and Password
                        var newUser = new ZammadUser
                        {
                            Zone = _zone, 
                            Firstname  = Citizen.Names,
                            Lastname = lastName,
                            Email = _email,
                            //Attributes = attributes,
                            Cedula = Citizen.Id, 
                            Password = _password,
                            Organization = Organization.Name
                        };

                        var response = await _authService.SignUp(newUser);

                        if (response.doLogin || response.result)
                        {
                            var resp = await _authService.Login(_email);

                            if (resp.result)
                                await _navigationService.NavigateAsync($"/{NavigationKeys.HomePage}");
                            else if(resp.doSignUp)
                                await _dialogService.DisplayAlertAsync("", "No pudimos completar el proceso, intenta de nuevo más tarde.", "Ok");
                            else if (response.result)
                                await _navigationService.NavigateAsync($"/{NavigationKeys.LoginPage}");
                            else
                                await _dialogService.DisplayAlertAsync("", "No pudimos completar el proceso, intenta iniciar sesión más tarde.", "Ok");
                            return;
                        }                        
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync("", "No pudimos tomar tu correo del proveedor de identidad. Asegúrate de que tu correo sea público.", "Ok");
                    }
                }
            }
            catch (OperationCanceledException ocex)
            {
                Crashes.TrackError(ocex);
                Console.WriteLine("Autenticación canceledada.");
                await loadingDialog?.DismissAsync();

                await _dialogService.DisplayAlertAsync("", "Autenticación canceledada.", "Ok");
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                _logger.Report(ex);

                await _dialogService.DisplayAlertAsync("", $"Ocurrió un error: {ex.Message}", "Ok");
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                Citizen = parameters.GetValue<Citizen>("Citizen");
                _document = Citizen.Id;
                _password = Citizen.Id;
                _zone = parameters.GetValue<string>("ZoneCode");
                Organization = parameters.GetValue<Organization>("Organization");
                //_organizations = parameters.GetValue<List<Organization>>("Organizations");

                var locationParts = new string[]
                {
                    parameters.GetValue<string>("Region"),
                    parameters.GetValue<string>("Province"),
                    parameters.GetValue<string>("Municipality"),
                    parameters.GetValue<string>("District"),
                };
                Location = string.Join(", ", locationParts.Where(x => !string.IsNullOrEmpty(x)));
            }
        }
    }
}