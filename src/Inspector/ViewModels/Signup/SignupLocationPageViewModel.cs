using Inspector.Framework.Dtos;
using Inspector.Framework.Dtos.Zammad;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Prism.Commands;
using Prism.Logging;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels.Signup
{
    public class SignupLocationPageViewModel : TerritorialDivisionViewModel
    {
        public Citizen Citizen { get; set; }
        public string FullName { get; set; }
        public ObservableCollection<Group> Groups { get; set; }
        public Validatable<Group> Group { get; set; }

        IZammadLiteApi _zammadLiteApi;
        ICitizenAPI _citizenClient;
        ILogger _logger;
        public SignupLocationPageViewModel(INavigationService navigationService, IPageDialogService dialogService,
            ICacheService cacheService, ITerritorialDivisionAPI territorialDivisionClient, ILogger logger,
            ICitizenAPI citizenClient, IZammadLiteApi zammadLiteApi) :
            base(navigationService, dialogService, logger, cacheService, territorialDivisionClient)
        {
            _citizenClient = citizenClient;
            _zammadLiteApi = zammadLiteApi;
            _logger = logger;
            Group = Validator.Build<Group>().IsRequired(Message.FieldRequired);

            SelectGroupCommand = new DelegateCommand<Group>(group => SelectGroupCommandExecute(group));
            ValidateLocationCommand = new DelegateCommand(OnValidateLocationCommandExecute);
            SelectDistrictCommand = new DelegateCommand<Zone>(zone => District.Value = zone);
        }

        public ICommand ValidateLocationCommand { get; set; }
        public ICommand SelectGroupCommand { get; set; }
        public new ICommand SelectDistrictCommand { get; set; }


        private void SelectGroupCommandExecute(Group group)
        {
            Group.Value = group;
        }

        async void OnValidateLocationCommandExecute()
        {
            if (!Region.Validate())
            {
                return;
            }
            IsBusy = true;

            try
            {

                var parameters = new NavigationParameters();
                parameters.Add("Citizen", Citizen);
                parameters.Add("Group", Group.Value);
                parameters.Add("Groups", Groups.ToList());
                parameters.Add("Region", Region.Value?.Name ?? string.Empty);
                parameters.Add("Province", Province.Value?.Name ?? string.Empty);
                parameters.Add("Municipality", Municipality.Value?.Name ?? string.Empty);
                parameters.Add("District", District.Value?.Name ?? string.Empty);

                var zoneCode = string.Empty;

                if (District.Value != null)
                {
                    zoneCode = District.Value.Id;
                }
                else if (Municipality.Value != null)
                {
                    zoneCode = Municipality.Value.Id;
                }
                else if (Province.Value != null)
                {
                    zoneCode = Province.Value.Id;
                }
                else if (Region.Value != null)
                {
                    zoneCode = Region.Value.Id;
                }

                parameters.Add("ZoneCode", zoneCode);

                await _navigationService.NavigateAsync("SignupSocialMediaPage", parameters);
            }
            catch (System.Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
            }

            IsBusy = false;
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("Citizen"))
            {
                Citizen = parameters.GetValue<Citizen>("Citizen");
                FullName = $"{Citizen.Names} {Citizen.FirstSurname} {Citizen.SecondSurname}";
                await LoadRegions();

                var account = ZammadAccount.CreateTokenAccount(AppKeys.ZammadApiBaseUrl, AppKeys.ZammadToken);
                var groupClient = account.CreateGroupClient();
                var groups = await groupClient.GetGroupListAsync();
                Groups = new ObservableCollection<Group>(groups.Where(x => x.Active).OrderBy(x => x.Name));
            }
        }
    }
}