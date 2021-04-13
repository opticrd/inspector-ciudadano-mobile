using System;
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

        public BaseViewModel(INavigationService navigationService, IPageDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

        public string Title { get; set; }
        public bool IsBusy { get; set; }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
        }
    }
}