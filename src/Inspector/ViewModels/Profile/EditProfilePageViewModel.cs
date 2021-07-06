
using Inspector.Framework.Services;
using Prism.Navigation;
using Prism.Services;

namespace Inspector.ViewModels
{
    public class EditProfilePageViewModel : BaseViewModel
    {
        public EditProfilePageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {

        }
    }
}
