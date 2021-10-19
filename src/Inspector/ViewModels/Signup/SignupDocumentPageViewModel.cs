using Inspector.Framework.Dtos.Zammad;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Plugin.ValidationRules.Formatters;
using Plugin.ValidationRules.Rules;
using Prism.Commands;
using Prism.Logging;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using UIModule.Helpers.Rules;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;
using Zammad.Client;

namespace Inspector.ViewModels
{
    public class SignupDocumentPageViewModel : BaseViewModel
    {
        ILogger _logger;
        IAuthService _authService;
        public Validatable<string> Document { get; set; }


        ICitizenAPI _citizenClient;
        //ValidationUnit _validationUnit;
        public SignupDocumentPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ILogger logger, IAuthService authService,
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi, ICitizenAPI citizenClient, ITerritorialDivisionAPI territorialDivisionClient)
            : base(navigationService, dialogService, cacheService)
        {
            _citizenClient = citizenClient;
            _logger = logger;
            _authService = authService;

            Document = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule());
            Document.ValueFormatter = new MaskFormatter("XXX-XXXXXXX-X");
  
            ValidateDocumentCommand = new DelegateCommand(OnValidateDocumentCommandExecute);
        }

        async void OnValidateDocumentCommandExecute()
        {
            if (!Document.Validate())
            {
                await _dialogService.DisplayAlertAsync("", "Debes proveer un documento de identidad válido.", "Ok");
                return;
            }

            try
            {
                var documentContext = new Dictionary<string, string>();
                documentContext.Add("documento", Document.Value);
                documentContext.Add("device name", DeviceInfo.Name);
                documentContext.Add("device platfor", DeviceInfo.Platform.ToString());
                documentContext.Add("device version", DeviceInfo.VersionString);
                documentContext.Add("device manufacturer", DeviceInfo.Manufacturer);

                Analytics.TrackEvent("Validando documento", documentContext);
                using (await MaterialDialog.Instance.LoadingDialogAsync(message: "Por favor, espere..."))
                {
                    var info = await _citizenClient.GetCitizenBasicInfo(Document.Value.Replace("-", ""));

                    if (info != null && info.Valid)
                    {
                        var resp = await _authService.UserExist(info.Payload.Id, SearchParameter.Id);

                        if (resp.exist)
                        {
                            _logger.TrackEvent("UserExistTryingToSignUp");

                            var loginParameters = new NavigationParameters();
                            loginParameters.Add("Document", Document.Value);

                            await _dialogService.DisplayAlertAsync("Ya existe un usuario con tu cédula.", "La cédula que digitaste ya existe. Te redirigiremos para que inicies sesión con tus redes.", "Ok");
                            await _navigationService.NavigateAsync($"/{NavigationKeys.LoginPage}", loginParameters);
                            return;
                        }

                        var parameters = new NavigationParameters();
                        parameters.Add("Citizen", info.Payload);

                        await _navigationService.NavigateAsync(NavigationKeys.SignupLocationPage, parameters);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Crashes.TrackError(ex);
                _logger.Report(ex);
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
            }
        }
        public ICommand ValidateDocumentCommand { get; set; }
    }
}
