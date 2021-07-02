
using Inspector.Resources.Labels;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Windows.Input;
using UIModule.Helpers.Rules;

namespace Inspector.ViewModels
{
    public class AddReportPageViewModel : BaseViewModel
    {
        public AddReportPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService, dialogService)
        {
            ID = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule());

            Address = Validator.Build<string>().IsRequired(Message.FieldRequired);

            ReportCommand = new DelegateCommand(ReportCommandExecute);
        }

       
        public Validatable<string> ID { get; set; }
        public Validatable<string> Address { get; set; }

        public int StateSelected { get; set; }
        public int IncidentSelected { get; set; }
        public int CategorySelected { get; set; }
        public int xSelected { get; set; }
        public string CitizenName { get; set; }
        public string PhoneNumber { get; set; }
        public int InstitutionSelected { get; set; }
        public string Comments { get; set; }


        public ICommand ReportCommand { get; set; }

        private void ReportCommandExecute()
        {
            throw new NotImplementedException();
        }

    }
}
