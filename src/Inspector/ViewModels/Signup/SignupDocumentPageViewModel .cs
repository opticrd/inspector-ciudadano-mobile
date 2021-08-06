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
using System.Threading.Tasks;
using System.Windows.Input;
using UIModule.Helpers.Rules;
using Xamarin.Essentials;
using Xamarin.Forms;
using Zammad.Client;

namespace Inspector.ViewModels
{
    public class SignupDocumentPageViewModel : BaseViewModel
    {
        IKeycloakApi _keycloakApi;
        IZammadLiteApi _zammadLiteApi;

        public Validatable<string> Document { get; set; }
        public Validatable<string> Email { get; set; }
        public ICommand GoogleCommand { get; set; }
        public ICommand FacebookCommand { get; set; }

        public SignupDocumentPageViewModel(INavigationService navigationService, IPageDialogService dialogService,
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi)
            : base(navigationService, dialogService, cacheService)
        {
            _keycloakApi = keycloakApi;
            _zammadLiteApi = zammadLiteApi;

            Document = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule());

            Email = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .IsEmail(Message.InvalidEmail);

            GoogleCommand = new Command(async () => await OnAuthenticate("google"));
            FacebookCommand = new Command(async () => await OnAuthenticate("facebook"));

            ValidateDocumentCommand = new DelegateCommand(OnValidateDocumentCommandExecute);
        }

        public string AuthToken { get; set; }

        async void OnValidateDocumentCommandExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {

            }
            catch (System.Exception ex)
            {
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
            }

            IsBusy = false;
        }
        public ICommand ValidateDocumentCommand { get; set; }

        private async Task OnAuthenticate(string scheme)
        {
            try
            {
                WebAuthenticatorResult result = null;
                /*
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
                {*/
                var authenticationUrl = $"https://auth.digital.gob.do/auth/realms/master/broker/{scheme}/login?client_id=security-admin-console&tab_id=5mSdAyXt6dU&session_code=";

                var authUrl = new Uri(authenticationUrl);
                var callbackUrl = new Uri("ogticapp://");


                result = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl);
                //}
                
                AuthToken = string.Empty;

                if (result.Properties.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name))
                    AuthToken += $"Name: {name}{Environment.NewLine}";
                if (result.Properties.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
                    AuthToken += $"Email: {email}{Environment.NewLine}";

                AuthToken += result?.AccessToken ?? result?.IdToken;

                if (!string.IsNullOrWhiteSpace(AuthToken))
                {
                    await _navigationService.NavigateAsync("NavigationPage/HomePage");
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("", $"Login Failed.", "Ok");
                }
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
        }
    }
}
