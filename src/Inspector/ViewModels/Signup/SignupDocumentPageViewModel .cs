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

        /*
        public Validatable<string> Password { get; set; }
        public Validatable<string> ConfirmPassword { get; set; }
        */

        ICitizenAPI _citizenClient;
        //ValidationUnit _validationUnit;
        public SignupDocumentPageViewModel(INavigationService navigationService, IPageDialogService dialogService,
            ICacheService cacheService, IKeycloakApi keycloakApi, IZammadLiteApi zammadLiteApi, ICitizenAPI citizenClient,
            ITerritorialDivisionAPI territorialDivisionClient)
            : base(navigationService, dialogService, cacheService)
        {
            _citizenClient = citizenClient;

            Document = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule());
            /*
            Password = Validator.Build<string>()
                .IsRequired(Message.FieldRequired);
            
            ConfirmPassword = Validator.Build<string>()
                .IsRequired(Message.FieldRequired);

            _validationUnit = new ValidationUnit( Document, Password, ConfirmPassword, District);
*/
            ValidateDocumentCommand = new DelegateCommand(OnValidateDocumentCommandExecute);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

           // await LoadRegions();
        }
        async void OnValidateDocumentCommandExecute()
        {
            if (IsBusy)
                return;

//            _validationUnit.Validate();

            if (!Document.Validate())
            {
                await _dialogService.DisplayAlertAsync("", "Debes proveer un documento de identidad válido.", "Ok");
                return;
            }
            /*if(!_validationUnit.IsValid)
            {
                return;
            }

            if (Password.Value != ConfirmPassword.Value)
            {
                await _dialogService.DisplayAlertAsync("", "Los campos de contraseña deben ser iguales.", "Ok");
                return;
            }*/

            IsBusy = true;

            try
            {
                var info = await _citizenClient.GetCitizenBasicInfo(Document.Value);

                if (info != null && info.Valid)
                {
                    var parameters = new NavigationParameters();
                    parameters.Add("Citizen", info.Payload);
                    /*
                    parameters.Add("Password", Password.Value);
                    parameters.Add("Region", Region.Value.Name);
                    parameters.Add("Province", Province.Value.Name);
                    parameters.Add("Municipality", Municipality.Value.Name);
                    parameters.Add("District", District.Value.Name);
                    parameters.Add("ZoneCode", District.Value.Code);
                   */
                    //await _navigationService.NavigateAsync("SignupSocialMediaPage", parameters);
                    await _navigationService.NavigateAsync("SignupLocationPage", parameters);
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
