using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Plugin.ValidationRules.Rules;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using Zammad.Client;

namespace Inspector.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public LoginPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            Email = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .IsEmail(Message.InvalidEmail);

            Password = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .Must(x => x.Length > 4);

            LoginCommand = new DelegateCommand(OnLoginCommandExecute);
            ForgetPasswordCommand = new DelegateCommand(()=> dialogService.DisplayAlertAsync(General.ForgetPassword, "Contacte su supervisor para mas información.", "Ok"));
        }

        public Validatable<string> Password { get; set; }
        public Validatable<string> Email { get; set; }

        public ICommand LoginCommand { get; set; }
        public ICommand ForgetPasswordCommand { get; set; }

        async void OnLoginCommandExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (!Email.Validate() || !Password.Validate())
            {
                IsBusy = false;
                return;
            }
                
            try
            {
                var account = ZammadAccount.CreateBasicAccount(AppKeys.ZammadApiBaseUrl, Email.Value, Password.Value);
                var client = account.CreateUserClient();
                var userAccount = await client.GetUserMeAsync();

                if (userAccount.Active)
                {
                    Settings.IsLoggedIn = true;
                    await _cacheService.InsertSecureObject(CacheKeys.ZammadAccount, account);
                    await _cacheService.InsertSecureObject(CacheKeys.UserAccount, userAccount);

                    await _navigationService.NavigateAsync(NavigationKeys.HomePage);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("", Message.AccountNotActivated, "Ok");
                }                    
            }
            catch (System.Exception)
            {
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
            }
            
            IsBusy = false;
        }
    }
}
