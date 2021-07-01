using Inspector.Resources.Labels;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Inspector.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public LoginPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService, dialogService)
        {
            Email = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .IsEmail(Message.InvalidEmail);

            Password = Validator.Build<string>().IsRequired(Message.FieldRequired);

            LoginCommand = new DelegateCommand(OnLoginCommandExecute);
            ForgetPasswordCommand = new DelegateCommand(()=> dialogService.DisplayAlertAsync(General.ForgetPassword, "Contacte su supervisor para mas información.", "Ok"));
        }

        public Validatable<string> Password { get; set; }
        public Validatable<string> Email { get; set; }

        public ICommand LoginCommand { get; set; }
        public ICommand ForgetPasswordCommand { get; set; }

        async void OnLoginCommandExecute()
        {
            if (Email.Validate() & Email.Validate())
            {
                //TODO: Call the server here
                await _navigationService.NavigateAsync("NavigationPage/HomePage");
            }
        }
    }
}
