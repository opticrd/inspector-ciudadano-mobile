
using Inspector.Framework.Services;
using Prism.Navigation;
using Prism.Services;

namespace Inspector.ViewModels
{
    public class MainTabbedPageViewModel : BaseViewModel
    {
        public MainTabbedPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {

        }
    }
}
