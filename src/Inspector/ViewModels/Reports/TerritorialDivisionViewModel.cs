using Inspector.Framework.Dtos;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class TerritorialDivisionViewModel : BaseViewModel
    {
        ITerritorialDivisionAPI _territorialDivisionClient;
        public TerritorialDivisionViewModel(INavigationService navigationService, IPageDialogService dialogService,
            ICacheService cacheService, ITerritorialDivisionAPI territorialDivisionClient) : base(navigationService, dialogService, cacheService)
        {
            _territorialDivisionClient = territorialDivisionClient;
            Init();
        }

        public ObservableCollection<Zone> Regions { get; set; }
        public ObservableCollection<Zone> Provinces { get; set; }
        public ObservableCollection<Zone> Municipalities { get; set; }
        public ObservableCollection<Zone> Districts { get; set; }

        public Validatable<Zone> Region { get; set; }
        public Validatable<Zone> Province { get; set; }
        public Validatable<Zone> Municipality { get; set; }
        public Validatable<Zone> District { get; set; }

        public bool RegionIsEnabled { get; set; } = true;
        public bool ProvinceIsEnabled { get; set; } = true;
        public bool MunicipalityIsEnabled { get; set; } = true;
        public bool DistrictIsEnabled { get; set; } = true;

        public ICommand SelectRegionCommand { get; set; }
        public ICommand SelectProvinceCommand { get; set; }
        public ICommand SelectMunicipalityCommand { get; set; }
        public ICommand SelectDistrictCommand { get; set; }

        private async void Init()
        {
            Region = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            Province = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            Municipality = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            District = Validator.Build<Zone>().IsRequired(Message.FieldRequired);

            SelectRegionCommand = new DelegateCommand<Zone>(async zone =>
            {
                Region.Value = zone;
                Provinces = Municipalities = Districts = null;
                await SearchProvince(zone.Code);
            });

            SelectProvinceCommand = new DelegateCommand<Zone>(async zone =>
            {
                Province.Value = zone;
                Municipalities = Districts = null;
                await SearchMunicipality(Region.Value.Code, zone.Code);
            });

            SelectMunicipalityCommand = new DelegateCommand<Zone>(async zone =>
            {
                Municipality.Value = zone;
                Districts = null;
                await SearchDistrict(Region.Value.Code, Province.Value.Code, zone.Code);
            });

            SelectDistrictCommand = new DelegateCommand<Zone>(zone => District.Value = zone);

            bool valid = false;

            try
            {
                var userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);

                if (userAccount?.CustomAttributes["zone"] == null)
                    return;

                var response = await _territorialDivisionClient.GetHierarchy(userAccount.CustomAttributes["zone"].ToString());

                if (!response.Valid)
                    return;

                switch (response.Data.Level)
                {
                    case ZoneLevel.Region:
                        await LoadRegions();
                        SelectRegionCommand.Execute(Regions.SingleOrDefault(i => i.Code == response.Data.Region));
                        RegionIsEnabled = false;
                        break;
                    case ZoneLevel.Province:
                        await LoadRegions();
                        SelectRegionCommand.Execute(Regions.SingleOrDefault(i => i.Code == response.Data.Region));
                        RegionIsEnabled = false;

                        SelectProvinceCommand.Execute(Provinces.SingleOrDefault(i => i.Code == response.Data.Province));
                        ProvinceIsEnabled = false;
                        break;
                    case ZoneLevel.Municipality:
                        await LoadRegions();
                        SelectRegionCommand.Execute(Regions.SingleOrDefault(i => i.Code == response.Data.Region));
                        RegionIsEnabled = false;

                        SelectProvinceCommand.Execute(Provinces.SingleOrDefault(i => i.Code == response.Data.Province));
                        ProvinceIsEnabled = false;

                        SelectMunicipalityCommand.Execute(Municipalities.SingleOrDefault(i => i.Code == response.Data.Municipality));
                        MunicipalityIsEnabled = false;
                        break;
                    case ZoneLevel.District:
                        await LoadRegions();
                        SelectRegionCommand.Execute(Regions.SingleOrDefault(i => i.Code == response.Data.Region));
                        RegionIsEnabled = false;

                        SelectProvinceCommand.Execute(Provinces.SingleOrDefault(i => i.Code == response.Data.Province));
                        ProvinceIsEnabled = false;

                        SelectMunicipalityCommand.Execute(Municipalities.SingleOrDefault(i => i.Code == response.Data.Municipality));
                        MunicipalityIsEnabled = false;

                        SelectDistrictCommand.Execute(Districts.SingleOrDefault(i => i.Code == response.Data.District));
                        DistrictIsEnabled = false;
                        break;
                    //case ZoneLevel.Section:
                    //    break;
                    //case ZoneLevel.Neighborhood:
                    //    break;
                    //case ZoneLevel.SubNeighborhood:
                    //    break;
                    default:
                        break;
                }

                valid = true;
            }
            catch { }
            finally
            {
                if (!valid)
                {
                    await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
                    await _navigationService.GoBackAsync();
                }
            }
        }

        private async Task LoadRegions()
        {
            var result = await _territorialDivisionClient.GetRegions();

            if (result.Valid)
                Regions = new ObservableCollection<Zone>(result.Data);
        }

        private async Task SearchProvince(string id)
        {
            var result = await _territorialDivisionClient.GetRegionProvince(id);

            if (result.Valid)
                Provinces = new ObservableCollection<Zone>(result.Data);
        }

        private async Task SearchMunicipality(string regionId, string provinceId)
        {
            var result = await _territorialDivisionClient.GetProvinceMunicipality(regionId, provinceId);

            if (result.Valid)
                Municipalities = new ObservableCollection<Zone>(result.Data);
        }

        private async Task SearchDistrict(string regionId, string provinceId, string municipalityId)
        {
            var result = await _territorialDivisionClient.GetMunicipalityDistrict(regionId, provinceId, municipalityId);

            if (result.Valid)
                Districts = new ObservableCollection<Zone>(result.Data);
        }

    }
}
