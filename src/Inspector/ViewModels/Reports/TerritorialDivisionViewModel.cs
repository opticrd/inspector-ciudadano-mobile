using Inspector.Framework.Dtos;
using Inspector.Framework.Helpers;
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
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class TerritorialDivisionViewModel : BaseViewModel
    {
        ITerritorialDivisionAPI _territorialDivisionClient;
        ILogger _logger;

        public TerritorialDivisionViewModel(INavigationService navigationService, IPageDialogService dialogService, ILogger logger,
            ICacheService cacheService, ITerritorialDivisionAPI territorialDivisionClient) : base(navigationService, dialogService, cacheService)
        {
            _territorialDivisionClient = territorialDivisionClient;
            _logger = logger;
            Init();
        }

        public ObservableCollection<Zone> Regions { get; set; }
        public ObservableCollection<Zone> Provinces { get; set; }
        public ObservableCollection<Zone> Municipalities { get; set; }
        public ObservableCollection<Zone> Districts { get; set; }
        public ObservableCollection<Zone> Sections { get; set; }
        public ObservableCollection<Zone> Neighhborhoods { get; set; }
        public ObservableCollection<Zone> SubNeighhborhoods { get; set; }

        public Validatable<Zone> Region { get; set; }
        public Validatable<Zone> Province { get; set; }
        public Validatable<Zone> Municipality { get; set; }
        public Validatable<Zone> District { get; set; }
        public Validatable<Zone> Section { get; set; }
        public Validatable<Zone> Neighhborhood { get; set; }
        public Validatable<Zone> SubNeighhborhood { get; set; }

        public bool RegionIsEnabled { get; set; } = true;
        public bool ProvinceIsEnabled { get; set; } = true;
        public bool MunicipalityIsEnabled { get; set; } = true;
        public bool DistrictIsEnabled { get; set; } = true;
        public bool SectionIsEnabled { get; set; } = true;
        public bool NeighhborhoodIsEnabled { get; set; } = true;
        public bool SubNeighhborhoodIsEnabled { get; set; } = true;
        public bool IsSearchingZones { get; set; }

        public ICommand SelectRegionCommand { get; set; }
        public ICommand SelectProvinceCommand { get; set; }
        public ICommand SelectMunicipalityCommand { get; set; }
        public ICommand SelectDistrictCommand { get; set; }
        public ICommand SelectSectionCommand { get; set; }
        public ICommand SelectNeighhborhoodCommand { get; set; }
        public ICommand SelectSubNeighhborhoodCommand { get; set; }

        private async void Init()
        {
            Region = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            Province = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            Municipality = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            District = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            Section = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            Neighhborhood = Validator.Build<Zone>().IsRequired(Message.FieldRequired);
            SubNeighhborhood = Validator.Build<Zone>().IsRequired(Message.FieldRequired);

            SelectRegionCommand = new DelegateCommand<Zone>(async zone => await SelectRegionCommandExecute(zone));
            SelectProvinceCommand = new DelegateCommand<Zone>(async zone => await SelectProvinceCommandExecute(zone));
            SelectMunicipalityCommand = new DelegateCommand<Zone>(async zone => await SelectMunicipalityCommandExecute(zone));
            SelectDistrictCommand = new DelegateCommand<Zone>(async zone => await SelectDistrictCommandExecute(zone));
            SelectSectionCommand = new DelegateCommand<Zone>(async zone => await SelectSectionCommandExecute(zone));
            SelectNeighhborhoodCommand = new DelegateCommand<Zone>(async zone => await SelectNeighborhoodsCommandExecute(zone));

            SelectSubNeighhborhoodCommand = new DelegateCommand<Zone>(zone => SubNeighhborhood.Value = zone);

            bool valid = false;

            try
            {
                var userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);

                if (userAccount == null)
                {
                    valid = true;
                    return;
                }
                if (userAccount?.CustomAttributes["zone"] == null)
                    return;

                var response = await _territorialDivisionClient.GetHierarchy(userAccount.CustomAttributes["zone"].ToString());

                if (!response.Valid)
                    return;

                switch (response.Data.Level)
                {
                    case ZoneLevel.Region:
                        await LoadRegions();
                        SelectRegionCommand.Execute(Regions.SingleOrDefault(i => i.Id == response.Data.Region));
                        RegionIsEnabled = false;
                        break;
                    case ZoneLevel.Province:
                        await LoadRegions();
                        await SelectRegionCommandExecute(Regions.SingleOrDefault(i => i.Id == response.Data.Region));
                        RegionIsEnabled = false;

                        await SelectProvinceCommandExecute(Provinces.SingleOrDefault(i => i.Id == response.Data.Province));
                        ProvinceIsEnabled = false;
                        break;
                    case ZoneLevel.Municipality:
                        await LoadRegions();
                        await SelectRegionCommandExecute(Regions.SingleOrDefault(i => i.Id == response.Data.Region));
                        RegionIsEnabled = false;

                        await SelectProvinceCommandExecute(Provinces.SingleOrDefault(i => i.Id == response.Data.Province));
                        ProvinceIsEnabled = false;

                        await SelectMunicipalityCommandExecute(Municipalities.SingleOrDefault(i => i.Id == response.Data.Municipality));
                        MunicipalityIsEnabled = false;
                        break;
                    case ZoneLevel.District:
                        await LoadRegions();
                        await SelectRegionCommandExecute(Regions.SingleOrDefault(i => i.Id == response.Data.Region));
                        RegionIsEnabled = false;

                        await SelectProvinceCommandExecute(Provinces.SingleOrDefault(i => i.Id == response.Data.Province));
                        ProvinceIsEnabled = false;

                        await SelectMunicipalityCommandExecute(Municipalities.SingleOrDefault(i => i.Id == response.Data.Municipality));
                        MunicipalityIsEnabled = false;

                        SelectDistrictCommand.Execute(Districts.SingleOrDefault(i => i.Id == response.Data.District));
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
            catch (Exception e)
            {
                _logger.Report(e);
            }
            finally
            {
                if (!valid)
                {
                    await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
                    await _navigationService.GoBackAsync();
                }
            }
        }
        protected Zone GetZone()
        {
            if (SubNeighhborhood.Value != null)
                return SubNeighhborhood.Value;

            if (Neighhborhood.Value != null)
                return Neighhborhood.Value;

            if (Section.Value != null)
                return Section.Value;

            if (District.Value != null)
                return District.Value;

            if (Municipality.Value != null)
                return Municipality.Value;

            if (Province.Value != null)
                return Province.Value;

            return null;
        }

        #region Searchs
        protected async Task LoadRegions()
        {
            try
            {
                IsSearchingZones = true;

                var result = await _territorialDivisionClient.GetRegions();

                if (result.Valid)
                    Regions = new ObservableCollection<Zone>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
            }
            finally
            {
                IsSearchingZones = false;
            }
        }

        private async Task SearchProvince(string regionId)
        {
            try
            {
                IsSearchingZones = true;

                var result = await _territorialDivisionClient.GetProvinces(new QueryZone { RegionCode = regionId });

                if (result.Valid)
                    Provinces = new ObservableCollection<Zone>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
            }
            finally
            {
                IsSearchingZones = false;
            }
        }

        private async Task SearchMunicipality(string regionId, string provinceId)
        {
            try
            {
                IsSearchingZones = true;

                var result = await _territorialDivisionClient.GetMunicipalities(new QueryZone { RegionCode = regionId, ProvinceCode = provinceId });

                if (result.Valid)
                    Municipalities = new ObservableCollection<Zone>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
            }
            finally
            {
                IsSearchingZones = false;
            }
        }

        private async Task SearchDistrict(string regionId, string provinceId, string municipalityId)
        {
            try
            {
                IsSearchingZones = true;

                var result = await _territorialDivisionClient.GetDistricts(new QueryZone { RegionCode = regionId, ProvinceCode = provinceId, MunicipalityCode = municipalityId });

                if (result.Valid)
                    Districts = new ObservableCollection<Zone>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
            }
            finally
            {
                IsSearchingZones = false;
            }
        }

        private async Task SearchSection(string regionId, string provinceId, string municipalityId, string districId)
        {
            try
            {
                IsSearchingZones = true;

                var result = await _territorialDivisionClient.GetSections(new QueryZone
                {
                    RegionCode = regionId,
                    ProvinceCode = provinceId,
                    MunicipalityCode = municipalityId,
                    DistrictCode = districId,
                });

                if (result.Valid)
                    Sections = new ObservableCollection<Zone>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
            }
            finally
            {
                IsSearchingZones = false;
            }
        }

        private async Task SearchNeighborhood(string regionId, string provinceId, string municipalityId, string districId, string sectionId)
        {
            try
            {
                IsSearchingZones = true;

                var result = await _territorialDivisionClient.GetNeighborhoods(new QueryZone
                {
                    RegionCode = regionId,
                    ProvinceCode = provinceId,
                    MunicipalityCode = municipalityId,
                    DistrictCode = districId,
                    SectionCode = sectionId
                });

                if (result.Valid)
                    Neighhborhoods = new ObservableCollection<Zone>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
            }
            finally
            {
                IsSearchingZones = false;
            }
        }

        private async Task SearchSubNeighborhood(string regionId, string provinceId, string municipalityId, string districId, string sectionId, string neighborhoodId)
        {
            try
            {
                IsSearchingZones = true;

                var result = await _territorialDivisionClient.GetSubNeighborhoods(new QueryZone
                {
                    RegionCode = regionId,
                    ProvinceCode = provinceId,
                    MunicipalityCode = municipalityId,
                    DistrictCode = districId,
                    SectionCode = sectionId,
                    NeighhborhoodCode = neighborhoodId,
                });

                if (result.Valid && result.Data != null)
                    SubNeighhborhoods = new ObservableCollection<Zone>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                // await _dialogService.DisplayAlertAsync(":(", Message.AssignedZoneNotProcessed, "Ok");
            }
            finally
            {
                IsSearchingZones = false;
            }
        }
        #endregion

        #region CommandExecute
        private async Task SelectRegionCommandExecute(Zone zone)
        {
            if (zone == null) 
                return;

            Region.Value = zone;
            Provinces = Municipalities = Districts = Sections = Neighhborhoods = SubNeighhborhoods = null;
            Province.Value = Municipality.Value = District.Value = Section.Value = Neighhborhood.Value = SubNeighhborhood.Value = null;
            await SearchProvince(Region.Value.Code);
        }

        private async Task SelectProvinceCommandExecute(Zone zone)
        {
            if (zone == null)
                return;

            Province.Value = zone;
            Municipalities = Districts = Sections = Neighhborhoods = SubNeighhborhoods = null;
            Municipality.Value = District.Value = Section.Value = Neighhborhood.Value = SubNeighhborhood.Value = null;
            await SearchMunicipality(Region.Value.Code, Province.Value.Code);
        }

        private async Task SelectMunicipalityCommandExecute(Zone zone)
        {
            if (zone == null)
                return;

            Municipality.Value = zone;
            Districts = Sections = Neighhborhoods = SubNeighhborhoods = null;
            District.Value = Section.Value = Neighhborhood.Value = SubNeighhborhood.Value = null;
            await SearchDistrict(Region.Value.Code, Province.Value.Code, Municipality.Value.Code);
        }

        private async Task SelectDistrictCommandExecute(Zone zone)
        {
            if (zone == null)
                return;

            District.Value = zone;
            Sections = Neighhborhoods = SubNeighhborhoods = null;
            Section.Value = Neighhborhood.Value = SubNeighhborhood.Value = null;
            await SearchSection(Region.Value.Code, Province.Value.Code, Municipality.Value.Code, District.Value.Code);
        }

        private async Task SelectSectionCommandExecute(Zone zone)
        {
            if (zone == null)
                return;

            Section.Value = zone;
            Neighhborhoods = SubNeighhborhoods = null;
            Neighhborhood.Value = SubNeighhborhood.Value = null;
            await SearchNeighborhood(Region.Value.Code, Province.Value.Code, Municipality.Value.Code, District.Value.Code, Section.Value.Code);
        }

        private async Task SelectNeighborhoodsCommandExecute(Zone zone)
        {
            if (zone == null)
                return;

            Neighhborhood.Value = zone;
            SubNeighhborhoods = null;
            SubNeighhborhood.Value = null;
            await SearchSubNeighborhood(Region.Value.Code, Province.Value.Code, Municipality.Value.Code, District.Value.Code, Section.Value.Code, Neighhborhood.Value.Code);
        }

        #endregion
    }
}
