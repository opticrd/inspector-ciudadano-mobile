
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Models;
using Inspector.Resources.Labels;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UIModule.Helpers.Rules;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class AddReportPageViewModel : BaseViewModel
    {
        TicketClient _ticketClient;
        User _userAccount;

        public AddReportPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            ID = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule());

            Address = Validator.Build<string>().IsRequired(Message.FieldRequired);

            States = new ObservableCollection<StateTicket>(StateTicket.GetStatesForNewTicket());

            ReportCommand = new DelegateCommand(ReportCommandExecute);
        }

        public Validatable<string> ID { get; set; }
        public Validatable<string> Address { get; set; }
        public ObservableCollection<StateTicket> States { get; set; }
        public int StateSelected { get; set; }
        public int IncidentSelected { get; set; }
        public int CategorySelected { get; set; }
        public int xSelected { get; set; }
        public string CitizenName { get; set; }
        public string PhoneNumber { get; set; }
        public int InstitutionSelected { get; set; }
        public string Comments { get; set; }

        public ICommand ReportCommand { get; set; }

        private async void Init()
        {
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _ticketClient = account.CreateTicketClient();

            _userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);
        }

        private async void ReportCommandExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            var ticket = await _ticketClient.CreateTicketAsync(
                new Ticket
                {
                    Title = "Help me!",
                    GroupId = 1,
                    CustomerId = 1,
                    OwnerId = 1,
                },
                new TicketArticle
                {
                    Subject = "Help me!!!",
                    Body = "Nothing Work!",
                    Type = "note",
                });

            IsBusy = false;
        }

    }
}
