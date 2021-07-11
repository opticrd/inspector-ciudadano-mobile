
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
using System.Collections.Generic;
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
        ValidationUnit _validationUnit;

        public AddReportPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            StateSelected = Validator.Build<int>()
                            .Must(x => x > 0, Message.FieldRequired);

            ID = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule());

            PhoneNumber = Validator.Build<string>()
                            .When(x => !string.IsNullOrEmpty(x))
                            .Must(x => x.Length >= 10, Message.InvalidPhoneNumber);

            Title = Validator.Build<string>()
                            .IsRequired(Message.FieldRequired)
                            .Must(x => x.Length >= 10, Message.InvalidField);

            Address = Validator.Build<string>()
                            .IsRequired(Message.FieldRequired)
                            .Must(x => x.Length >= 10, Message.InvalidField);

            GroupSelected = Validator.Build<int>()
                            .Must(x => x > 0, Message.FieldRequired);

            Comments = Validator.Build<string>()
                            .When(x => !string.IsNullOrEmpty(x))
                            .Must(x => x.Length >= 10, Message.InvalidField);

            _validationUnit = new ValidationUnit(StateSelected, ID, PhoneNumber, Title, Address, GroupSelected, Comments);

            States = new List<StateTicket>(StateTicket.GetStatesForNewTicket());

            ReportCommand = new DelegateCommand(ReportCommandExecute);
            SelectStateCommand = new DelegateCommand<StateTicket>(state => StateSelected.Value = state.State);
            SelectGroupCommand = new DelegateCommand<Group>(group => GroupSelected.Value = group.Id);

            Init();
        }

        public List<StateTicket> States { get; set; }
        public Validatable<int> StateSelected { get; set; }
        public Validatable<string> ID { get; set; }
        public string CitizenName { get; set; }
        public Validatable<string> PhoneNumber { get; set; }
        public Validatable<string> Title { get; set; }
        public Validatable<string> Address { get; set; }
        public int CategorySelected { get; set; }
        public List<Group> Groups { get; set; }
        public Validatable<int> GroupSelected { get; set; }
        public Validatable<string> Comments { get; set; }
        //public int IncidentSelected { get; set; }
        //public int xSelected { get; set; }
        //public int InstitutionSelected { get; set; }

        public ICommand ReportCommand { get; set; }
        public ICommand SelectStateCommand { get; set; }
        public ICommand SelectGroupCommand { get; set; }

        private async void Init()
        {
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _ticketClient = account.CreateTicketClient();

            _userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);
            Groups = await _cacheService.GetLocalObject<List<Group>>(CacheKeys.Groups);
        }

        private async void ReportCommandExecute()
        {
            if (IsBusy || !_validationUnit.Validate())
                return;

            IsBusy = true;

            try
            {
                var ticket = await _ticketClient.CreateTicketAsync(
                        new Ticket
                        {
                            Title = Title.Value,
                            GroupId = StateSelected.Value,
                            CustomerId = _userAccount.Id,
                            OwnerId = _userAccount.Id,
                            StateId = GroupSelected.Value,
                            CustomAttributes = new Dictionary<string, object>()
                            {
                                { "address",  Address.Value },
                            }
                        },
                        new TicketArticle
                        {
                            Subject = Title.Value,
                            Body = Comments.Value,
                            Type = "note",
                        });

                if (ticket.Id <= 0)                
                    await _dialogService.DisplayAlertAsync("", Message.TicketNotCreated, "Ok");
                else
                {
                    await _dialogService.DisplayAlertAsync(":)", Message.TicketCreated, "Ok");

                    var parameters = new NavigationParameters()
                    {
                        { "newTicket", ticket }
                    };
                    await _navigationService.GoBackAsync(parameters);
                }                         
            }
            catch (Zammad.Client.Core.ZammadException e)
            {
#if DEBUG || DEBUG_AGENT
                var content = await e.Response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
#endif
                await _dialogService.DisplayAlertAsync("Ups :(", Message.SomethingHappen, "Ok");
            }

            IsBusy = false;
        }

    }
}
