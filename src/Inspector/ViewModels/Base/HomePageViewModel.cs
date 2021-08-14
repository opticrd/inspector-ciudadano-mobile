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
        List<Ticket> _allTickets;
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
            LogoutCommand = new DelegateCommand(async () =>
            {
                Settings.RemoveAllSettings();

                await _cacheService.RemoveSecureObject(CacheKeys.ZammadAccount);
                await _cacheService.RemoveSecureObject(CacheKeys.UserAccount);

                await _navigationService.NavigateAsync("/" + NavigationKeys.LoginPage);
            });
        }

        public ObservableCollection<Ticket> Tickets { get; set; } = new ObservableCollection<Ticket>();
        public Ticket TicketSelected { get; set; }
        public int HistoryIndexSelected { get; set; }
        public User UserAccount { get; set; }

        #region Commands
        public ICommand RefreshCommand { get; set; }
        public ICommand LoadMoreItemsCommand { get; set; }
        public ICommand AllTicketsCommand { get; set; }
        public ICommand OpenTicketsCommand { get; set; }
        public ICommand ClosedTicketsCommand { get; set; }
        public ICommand TicketSelectedCommand { get; set; }
        public ICommand LogoutCommand { get; set; } 
        #endregion

        private async void Init()
        {
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _ticketClient = account.CreateTicketClient();

            UserAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);

            await Task.Run(OnRefreshCommandExecute);
        }

        private async void OnRefreshCommandExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            _page = 1;

            try
            {
                var query = "owner_id:" + UserAccount.Id + " OR " + "created_by_id:" + UserAccount.Id;
                var ticketList = await _ticketClient.SearchTicketAsync(query, 50);
                _allTickets = ticketList.ToList();

                //var ticketList = await _ticketClient.GetTicketListAsync(_page, 30);
                //_allTickets = ticketList.Where(x => x.OwnerId == UserAccount.Id).ToList();

                Tickets = new ObservableCollection<Ticket>(_allTickets);
            }
            catch { }
            finally
            {
                HistoryIndexSelected = 0;
                IsBusy = false;
            }
        }

        private async void OnLoadMoreItemsCommandsExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                _page++;
                var ticketList = await _ticketClient.GetTicketListAsync(_page, 30);
                var userTickets = ticketList.Where(x => x.OwnerId == UserAccount.Id);
                _allTickets = _allTickets.Concat(userTickets).ToList();

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
                        Tickets = new ObservableCollection<Ticket>(_allTickets);
                        break;
                }
            }
            catch { }
            finally
            {
                IsBusy = false;
            }            
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
                _allTickets = _allTickets.Concat(ticket).ToList();

                HistoryIndexSelected = 0;
                OnChangeHistoryTicketsFilter(0);
            }
        }
    }
}
