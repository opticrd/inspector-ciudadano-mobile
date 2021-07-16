using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Models;
using NativeMedia;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Inspector.ViewModels
{
    public class GalleryViewModel : BaseViewModel
    {
        public GalleryViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) 
            : base(navigationService, dialogService, cacheService)
        {
            
        }


        
    }
}
