using Inspector.Framework.Dtos;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
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
using XF.Material.Forms.UI.Dialogs;

namespace Inspector.ViewModels
{
    public class IncidentsViewModel : TerritorialDivisionViewModel
    {
        IIncidentsAPI _incidentsClient;
        ILogger _logger;
        public IncidentsViewModel(INavigationService navigationService, IPageDialogService dialogService, ILogger logger,
            ICacheService cacheService, IIncidentsAPI incidentsClient, ITerritorialDivisionAPI territorialDivisionClient)
            : base(navigationService, dialogService, logger, cacheService, territorialDivisionClient)
        {
            _logger = logger;
            _incidentsClient = incidentsClient;
            Init();
        }

        public ObservableCollection<Incident> Incidents { get; set; }
        public ObservableCollection<Incident> Categories { get; set; }
        public ObservableCollection<Incident> SubCategories { get; set; }

        public Validatable<Incident> Incident { get; set; }
        public Validatable<Incident> Category { get; set; }
        public Validatable<Incident> SubCategory { get; set; }

        public ICommand SelectIncidentCommand { get; set; }
        public ICommand SelectCategoryCommand { get; set; }
        public ICommand SelectSubCategoryCommand { get; set; }

        private async void Init()
        {
            Incident = Validator.Build<Incident>().IsRequired(Message.FieldRequired);
            Category = Validator.Build<Incident>().IsRequired(Message.FieldRequired);
            SubCategory = Validator.Build<Incident>().IsRequired(Message.FieldRequired);

            SelectIncidentCommand = new DelegateCommand<Incident>(tag =>
            {
                Incident.Value = tag;
                Categories = SubCategories = null;
                SearchCategories(tag.Id);
            });

            SelectCategoryCommand = new DelegateCommand<Incident>(tag =>
            {
                Category.Value = tag;
                SubCategories = null;
                SearchSubCategories(Incident.Value.Id, tag.Id);
            });

            SelectSubCategoryCommand = new DelegateCommand<Incident>(tag => SubCategory.Value = tag);

            await SearchIncidentType();
        }

        private async Task SearchIncidentType()
        {
            try
            {
                var result = await _incidentsClient.GetIncidentTypes();

                if (result.Valid)
                    Incidents = new ObservableCollection<Incident>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await MaterialDialog.Instance.SnackbarAsync(Message.IncidentNotLoaded, MaterialSnackbar.DurationLong);
            }
        }

        private async void SearchCategories(int incidentId)
        {
            try
            {
                var result = await _incidentsClient.GetCategories(incidentId);

                if (result.Valid)
                    Categories = new ObservableCollection<Incident>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await MaterialDialog.Instance.SnackbarAsync(Message.IncidentNotLoaded, MaterialSnackbar.DurationLong);
            }
        }

        private async void SearchSubCategories(int incidentId, int categoryId)
        {
            try
            {
                var result = await _incidentsClient.GetSubCategories(incidentId, categoryId);

                if (result.Valid)
                    SubCategories = new ObservableCollection<Incident>(result.Data.OrderBy(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await MaterialDialog.Instance.SnackbarAsync(Message.IncidentNotLoaded, MaterialSnackbar.DurationLong);
            }
        }
    }
}
