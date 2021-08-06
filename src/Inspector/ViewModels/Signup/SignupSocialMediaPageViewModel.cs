using Inspector.Framework.Dtos;
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
    public class SignupSocialMediaPageViewModel : BaseViewModel
    {
        IKeycloakApi _keycloakApi;
        IZammadLiteApi _zammadLiteApi;
        public ICommand GoogleCommand { get; set; }
        public ICommand FacebookCommand { get; set; }

        const string AuthenticationUrl = "https://citizens-auth-api-dev-i42qq4zxeq-ue.a.run.app/mobileauth/";

        private string _email;
        private string _document;
        private Citizen _citizen;
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
                _citizen = parameters.GetValue<Citizen>("Citizen");
                _document = _citizen.Id;
            }
        }

        private async Task OnAuthenticate(string scheme)
        {
            try
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
                    // Create user with email and Password
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
        }
    }
}
