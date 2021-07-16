using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Models;
using NativeMedia;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class ReportDetailPageViewModel : BaseViewModel
    {
        TicketClient _ticketClient;
        User _userAccount;

        public ReportDetailPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            //DeleteAllItemsCommand = new DelegateCommand(async ()=> 
            //{
            //    var parameters = new NavigationParameters()
            //    {
            //        { NavigationKeys.RemoveAllFiles, true }
            //    };
            //    await _navigationService.GoBackAsync(parameters);
            //});

            Init();
        }
        public Ticket TicketSelected { get; set; }
        public ObservableCollection<Comment> Comments { get; set; }
        public ICommand DeleteAllItemsCommand { get; set; }

        private async void Init()
        {
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);
            _ticketClient = account.CreateTicketClient();
        }

        private async void InitComments()
        {
            if (IsBusy && TicketSelected.ArticleCount == 0)
                return;

            IsBusy = true;

            try
            {
                var ticketArticles = await _ticketClient.GetTicketArticleListForTicketAsync(TicketSelected.Id);
                if (ticketArticles.Count == 0)
                    return;

                var comments = new List<Comment>();
                foreach (var item in ticketArticles)
                {
                    var owner = item.CreatedById == _userAccount.Id;
                    comments.Add(new Comment
                    {
                        UserName = item.From,
                        Body = item.Body,
                        CreatedAt = item.CreatedAt,
                        Attachments = item.Attachments,
                        IsOwner = owner
                    });
                }
                Comments = new ObservableCollection<Comment>(comments);
            }
            catch
            {

            }
            finally
            {
                IsBusy = false;
            }
        }


        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(NavigationKeys.TicketSelected))
            {
                TicketSelected = parameters.GetValue<Ticket>(NavigationKeys.TicketSelected);
                InitComments();
            }
        }

    }
}
