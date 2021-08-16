using Inspector.Framework.Dtos;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Resources.Labels;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Inspector.ViewModels.Signup
{
    public class SignupLocationPageViewModel : TerritorialDivisionViewModel
    {
        public ICommand ValidateLocationCommand { get; set; }

        public Citizen Citizen { get; set; }
        public string FullName { get; set; }

        ICitizenAPI _citizenClient;
        public SignupLocationPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService, ITerritorialDivisionAPI territorialDivisionClient, ICitizenAPI citizenClient) :
            base(navigationService, dialogService, cacheService, territorialDivisionClient)
        {
            _citizenClient = citizenClient;
            ValidateLocationCommand = new DelegateCommand(OnValidateLocationCommandExecute);
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            Citizen = parameters.GetValue<Citizen>("Citizen");
            FullName = $"{Citizen.Names} {Citizen.FirstSurname} {Citizen.SecondSurname}";
            await LoadRegions();
        }

        async void OnValidateLocationCommandExecute()
        {
            if (IsBusy)
                return;

            if (!District.Validate())
            {
                return;
            }
            IsBusy = true;

            try
            {

                var parameters = new NavigationParameters();
                parameters.Add("Citizen", Citizen);
                parameters.Add("Region", Region.Value.Name);
                parameters.Add("Province", Province.Value.Name);
                parameters.Add("Municipality", Municipality.Value.Name);
                parameters.Add("District", District.Value.Name);
                parameters.Add("ZoneCode", District.Value.Code);
                parameters.Add("Password", Citizen.Id);

                await _navigationService.NavigateAsync("SignupSocialMediaPage", parameters);
            }
            catch (System.Exception)
            {
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
            }

            IsBusy = false;
        }
    }
}