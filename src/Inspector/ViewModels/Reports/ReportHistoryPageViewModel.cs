
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class ReportHistoryPageViewModel : BaseViewModel
    {
        TicketClient _ticketClient;
        public ReportHistoryPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            Init();
        }

        public ObservableCollection<Ticket> Tickets { get; set; }

        private async void Init()
        {
            var account = await _cacheService.GetLocalObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _ticketClient = account.CreateTicketClient();

            var ticketList = await _ticketClient.GetTicketListAsync(1, 10);
            Tickets = new ObservableCollection<Ticket>(ticketList);
        }
    }
}
