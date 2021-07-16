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

namespace Inspector.ViewModels
{
    public class GalleryPageViewModel : GalleryViewModel
    {
        public GalleryPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            DeleteAllItemsCommand = new DelegateCommand(async ()=> 
            {
                var parameters = new NavigationParameters()
                {
                    { NavigationKeys.RemoveAllFiles, true }
                };
                await _navigationService.GoBackAsync(parameters);
                Attachements = null;
            });

        }
        public ICommand DeleteAllItemsCommand { get; set; }

        public ObservableCollection<Gallery> Attachements { get; set; }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(NavigationKeys.ShowFiles))
            {
                var files = parameters.GetValue<ObservableCollection<IMediaFile>>(NavigationKeys.ShowFiles);
                var listFiles = new List<Gallery>();

                foreach (var item in files)
                {
                    listFiles.Add(new Gallery
                    {
                        File = item,
                        Name = item.NameWithoutExtension,
                        Type = item.ContentType,
                    });
                }

                Attachements = new ObservableCollection<Gallery>(listFiles);
            }
        }
    }
}
