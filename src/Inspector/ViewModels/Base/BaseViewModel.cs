using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Inspector.Framework.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using PropertyChanged;

namespace Inspector.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class BaseViewModel : INavigationAware
    {
        protected IPageDialogService _dialogService;
        protected INavigationService _navigationService;
        protected ICacheService _cacheService;

        public BaseViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            NavigateCommand = new DelegateCommand<string>(NavigateCommandExecute);
        }

        public string Title { get; set; }
        public bool IsBusy { get; set; }


        public ICommand NavigateCommand { get; set; }

       private async void NavigateCommandExecute(string uri)
        {
            var result = await _navigationService.NavigateAsync(uri);

            if (!result.Success)
            {
                Console.WriteLine(result.Exception);
            }
        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
        }
    }
}