using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        TicketClient _ticketClient;
        User _userAccount;
        IEnumerable _allTickets;
        int _page = 1;
        public HomePageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            Init();
            RefreshCommand = new DelegateCommand(OnRefreshCommandExecute);
            LoadMoreItemsCommand = new DelegateCommand(OnLoadMoreItemsCommandsExecute);
        }

        public ObservableCollection<Ticket> Tickets { get; set; } = new ObservableCollection<Ticket>();
        public ICommand RefreshCommand { get; set; }
        public ICommand LoadMoreItemsCommand { get; set; }

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
            var _allTickets = ticketList.Where(x => x.OwnerId == _userAccount.Id);

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

            foreach (var item in userTickets)            
                Tickets.Add(item);
            
            IsBusy = false;
        }
    }
}
