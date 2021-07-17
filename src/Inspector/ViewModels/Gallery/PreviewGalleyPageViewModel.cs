using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class PreviewGalleyPageViewModel : GalleryViewModel
    {
        TicketClient _ticketClient;

        public PreviewGalleyPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService)
            : base(navigationService, dialogService, cacheService)
        {
            Init();
        }
        private async void Init()
        {
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _ticketClient = account.CreateTicketClient();
        }

        public ObservableCollection<MediaFile> Attachements { get; set; }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (!parameters.ContainsKey(NavigationKeys.CommentSelected))
                return;

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var comment = parameters.GetValue<Comment>(NavigationKeys.CommentSelected);
                var files = comment.Attachments;
                var listFiles = new List<MediaFile>();

                foreach (var item in files)
                {
                    FileType fileType;
                    string key = "";

                    if (item.Preferences.ContainsKey("Content-Type"))
                        key = "Content-Type";
                    else if (item.Preferences.ContainsKey("Mime-Type"))
                        key = "Mime-Type";
                    else
                        continue;

                    var type = (string)item.Preferences[key];

                    if (type.Contains("image"))
                        fileType = FileType.Image;
                    else if (type.Contains("video"))
                        fileType = FileType.Video;
                    else
                        continue;

                    var fileStream = await _ticketClient.GetTicketArticleAttachmentAsync(comment.TicketId, comment.Id, item.Id);
                    listFiles.Add(new MediaFile
                    {
                        Data = fileStream,
                        Type = fileType,
                        FileName = item.Filename
                    });
                }

                Attachements = new ObservableCollection<MediaFile>(listFiles);
            }
            catch (Exception)
            {
                await _dialogService.DisplayAlertAsync(":(", "Ups, algo ha pasado. Intentelo mas tarde.", "Ok");
            }
            finally 
            {
                IsBusy = false;
            }            
        }

        

    }
}

namespace NativeMedia
{
    partial class MediaFile { }
}
