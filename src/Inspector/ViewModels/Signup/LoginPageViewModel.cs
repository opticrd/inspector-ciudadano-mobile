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
using Prism.Logging;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using XF.Material.Forms.UI.Dialogs;
using Zammad.Client;

namespace Inspector.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        ILogger _logger;
        IAuthService _authService;
        public ICommand GoogleCommand { get; set; }
        public ICommand FacebookCommand { get; set; }
        public ICommand MicrosoftCommand { get; set; }

        const string AuthenticationUrl = "https://citizens-auth-api-dev-i42qq4zxeq-ue.a.run.app/mobileauth/";
        public LoginPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ILogger logger, IAuthService authService,
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi) 
            : base(navigationService, dialogService, cacheService)
        {
            _logger = logger;
            _authService = authService;


            GoogleCommand = new DelegateCommand(async () => await OnAuthenticate("Google"));
            FacebookCommand = new DelegateCommand(async () => await OnAuthenticate("Facebook"));
            MicrosoftCommand = new DelegateCommand(async () => await OnAuthenticate("Microsoft"));

        }

        private void OnSignupCommandExecute()
        {
            _navigationService.NavigateAsync("SignupDocumentPage");
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
                        email = email.Replace("%40", "@");

                        var response = await _authService.Login(email);

                        if (response.doSignUp)
                        {
                            var doSignUp = await _dialogService.DisplayAlertAsync("", "Debes registrar tu cuenta para iniciar sesión.", "Registrarme", "Ok");
                            if (doSignUp)                            
                                await _navigationService.NavigateAsync("/WelcomePage/SignupDocumentPage");
                            
                            return;
                        }

                        if (response.result)
                        {
                            await _navigationService.NavigateAsync($"/{NavigationKeys.HomePage}");
                            return;
                        }
                    }
                    else
                    {
                        string msj = "";
                        if(scheme.Equals("Facebook"))
                            msj = "Asegúrate de que tu correo sea público en la plataforma de Facebook.";
                        else
                            msj = "No pudimos tomar tu correo del proveedor de identidad. Asegúrate de que tu correo sea público.";

                        await _dialogService.DisplayAlertAsync("", msj, "Ok");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Autenticación canceledada.");
                await _dialogService.DisplayAlertAsync("", "Login cancelado.", "Ok");
            }
            catch (Exception ex)
            {
                _logger.Report(ex);
                await _dialogService.DisplayAlertAsync("", Message.SomethingHappen, "Ok");
            }
            finally
            {
                IsBusy = false;
            }
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
    }
}
