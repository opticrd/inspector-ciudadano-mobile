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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using UIModule.Helpers.Rules;
using Zammad.Client;

namespace Inspector.ViewModels
{
    public class SignupDocumentPageViewModel : BaseViewModel
    {
        IKeycloakApi _keycloakApi;
        IZammadLiteApi _zammadLiteApi;

        public Validatable<string> Document { get; set; }
        public Validatable<string> Email { get; set; }

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

            ValidateDocumentCommand = new DelegateCommand(OnValidateDocumentCommandExecute);
        }

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
    }
}
