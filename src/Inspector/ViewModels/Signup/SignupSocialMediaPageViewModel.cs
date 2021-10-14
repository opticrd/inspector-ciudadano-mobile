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
using Prism.Logging;
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
using XF.Material.Forms.UI.Dialogs;
using Zammad.Client;


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
        private ZammadGroup _group;
        private List<ZammadGroup> _groups;
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

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                Citizen = parameters.GetValue<Citizen>("Citizen");
                _document = Citizen.Id;
                _password = Citizen.Id;
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
                using (await MaterialDialog.Instance.LoadingDialogAsync(message: "Por favor, espere..."))
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
                            Password = _password
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
            catch (OperationCanceledException)
            {
                Console.WriteLine("Autenticación canceledada.");

                await _dialogService.DisplayAlertAsync("", "Autenticación canceledada.", "Ok");
            }
            catch (Exception ex)
            {
                _logger.Report(ex);

                await _dialogService.DisplayAlertAsync("", $"Ocurrió un error: {ex.Message}", "Ok");
            }
        }
    }
}