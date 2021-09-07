
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Windows.Input;

namespace Inspector.ViewModels
{
    public class ProfilePageViewModel : BaseViewModel
    {
        public ProfilePageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            LogoutCommand = new DelegateCommand(async () =>
            {
                Settings.RemoveAllSettings();

                await _cacheService.RemoveSecureObject(CacheKeys.ZammadAccount);
                await _cacheService.RemoveSecureObject(CacheKeys.UserAccount);

                await _navigationService.NavigateAsync("/" + NavigationKeys.LoginPage);
            });
        }
        public ICommand LogoutCommand { get; set; }
    }
}
