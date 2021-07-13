using Inspector.Framework.Services;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.ViewModels
{
    public class GalleryPageViewModel : BaseViewModel
    {
        public GalleryPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {

        }
    }
}
