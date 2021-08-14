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

        public Validatable<string> Document { get; set; }
        ICitizenAPI _citizenClient;
        public SignupDocumentPageViewModel(INavigationService navigationService, IPageDialogService dialogService,
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi, ICitizenAPI citizenClient)
            : base(navigationService, dialogService, cacheService)
        {
            _citizenClient = citizenClient;

            Document = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule());

            ValidateDocumentCommand = new DelegateCommand(OnValidateDocumentCommandExecute);
        }

        public string AuthToken { get; set; }

        async void OnValidateDocumentCommandExecute()
        {
            if (IsBusy)
                return;

            if (!Document.Validate())
            {
                await _dialogService.DisplayAlertAsync("", "Debes proveer un documento de identidad válido.", "Ok");
                return;
            }

            IsBusy = true;

            try
            {
                var info = await _citizenClient.GetCitizenBasicInfo(Document.Value);

                if (info != null && info.Valid)
                {
                    var parameters = new NavigationParameters();
                    parameters.Add("Citizen", info.Payload);
                    await _navigationService.NavigateAsync("SignupSocialMediaPage", parameters);
                }
            }
            catch (System.Exception ex)
            {
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
            }

            IsBusy = false;
        }
        public ICommand ValidateDocumentCommand { get; set; }

      
    }
}
