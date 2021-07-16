using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Zammad.Client;
using Zammad.Client.Resources;
using TicketState = Inspector.Framework.Dtos.TicketState;

namespace Inspector.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        TicketClient _ticketClient;
        User _userAccount;
        IEnumerable<Ticket> _allTickets;
        int _page = 1;
        public HomePageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            Init();
            RefreshCommand = new DelegateCommand(OnRefreshCommandExecute);
            LoadMoreItemsCommand = new DelegateCommand(OnLoadMoreItemsCommandsExecute);
            AllTicketsCommand = new DelegateCommand(() => OnChangeHistoryTicketsFilter(0));
            OpenTicketsCommand = new DelegateCommand(() => OnChangeHistoryTicketsFilter(1));
            ClosedTicketsCommand = new DelegateCommand(() => OnChangeHistoryTicketsFilter(2));
            TicketSelectedCommand = new DelegateCommand(async () =>
            {
                var parameters = new NavigationParameters()
                {
                    { NavigationKeys.TicketSelected, TicketSelected }
                };
                await _navigationService.NavigateAsync(NavigationKeys.ReportDetailPage, parameters);
            });
        }
                
        public ObservableCollection<Ticket> Tickets { get; set; } = new ObservableCollection<Ticket>();
        public Ticket TicketSelected { get; set; }
        public int HistoryIndexSelected { get; set; }

        public ICommand RefreshCommand { get; set; }
        public ICommand LoadMoreItemsCommand { get; set; }
        public ICommand AllTicketsCommand { get; set; }
        public ICommand OpenTicketsCommand { get; set; }
        public ICommand ClosedTicketsCommand { get; set; }
        public ICommand TicketSelectedCommand { get; set; }

        private async void Init()
        {
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _ticketClient = account.CreateTicketClient();

            _userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);

            await Task.Run(OnRefreshCommandExecute);
        }

        private async void OnRefreshCommandExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            _page = 1;

            var ticketList = await _ticketClient.GetTicketListAsync(_page, 30);
            _allTickets = ticketList.Where(x => x.OwnerId == _userAccount.Id);

            Tickets = new ObservableCollection<Ticket>(_allTickets);

            IsBusy = false;
        }

        private async void OnLoadMoreItemsCommandsExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            _page++; 
            var ticketList = await _ticketClient.GetTicketListAsync(_page, 30);
            var userTickets = ticketList.Where(x => x.OwnerId == _userAccount.Id);

            switch (HistoryIndexSelected)
            {
                case 1:
                    var openTickets = userTickets.Where(x => x.StateId == (int)TicketState.Open);

                    foreach (var item in openTickets)
                        Tickets.Add(item);
                    break;
                case 2:
                    var closedTickets = userTickets.Where(x => x.StateId == (int)TicketState.Closed);

                    foreach (var item in closedTickets)
                        Tickets.Add(item);
                    break;
                default:
                    Tickets = new ObservableCollection<Ticket>(userTickets);
                    break;
            }

            IsBusy = false;
        }

        private void OnChangeHistoryTicketsFilter(int index)
        {
            switch (index)
            {
                case 1:
                    Tickets = new ObservableCollection<Ticket>(_allTickets.Where(x => x.StateId == (int)TicketState.Open));
                    break;
                case 2:
                    Tickets = new ObservableCollection<Ticket>(_allTickets.Where(x => x.StateId == (int)TicketState.Closed));
                    break;
                default:
                    Tickets = new ObservableCollection<Ticket>(_allTickets);
                    break;
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back && parameters.ContainsKey(NavigationKeys.NewTicket))
            {
                var ticket = parameters.GetValues<Ticket>(NavigationKeys.NewTicket);
                _allTickets = _allTickets.Concat(ticket);
                HistoryIndexSelected = 0;
                OnChangeHistoryTicketsFilter(0);
            }
        }
    }
}
