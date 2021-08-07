using Inspector.Framework.Dtos;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Resources.Labels;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

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

            SelectRegionCommand = new DelegateCommand<Zone>(zone =>
            {
                Region.Value = zone;
                Provinces = Municipalities = Districts = null;
                SearchProvince(zone.Code);
            });

            SelectProvinceCommand = new DelegateCommand<Zone>(zone =>
            {
                Province.Value = zone;
                Municipalities = Districts = null;
                SearchMunicipality(Region.Value.Code, zone.Code);
            });

            SelectMunicipalityCommand = new DelegateCommand<Zone>(zone =>
            {
                Municipality.Value = zone;
                Districts = null;
                SearchDistrict(Region.Value.Code, Province.Value.Code, zone.Code);
            });

            SelectDistrictCommand = new DelegateCommand<Zone>(zone => District.Value = zone);

            var result = await _territorialDivisionClient.GetRegions();

            if (result.Valid)
               Regions = new ObservableCollection<Zone>(result.Data);
        }

        private async void SearchProvince(string id)
        {
            var result = await _territorialDivisionClient.GetRegionProvince(id);

            if (result.Valid)
                Provinces = new ObservableCollection<Zone>(result.Data);
        }

        private async void SearchMunicipality(string regionId, string provinceId)
        {
            var result = await _territorialDivisionClient.GetProvinceMunicipality(regionId, provinceId);

            if (result.Valid)
                Municipalities = new ObservableCollection<Zone>(result.Data);
        }

        private async void SearchDistrict(string regionId, string provinceId, string municipalityId)
        {
            var result = await _territorialDivisionClient.GetMunicipalityDistrict(regionId, provinceId, municipalityId);

            if (result.Valid)
                Districts = new ObservableCollection<Zone>(result.Data);
        }

    }
}
