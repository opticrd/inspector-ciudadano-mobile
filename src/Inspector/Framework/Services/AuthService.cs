using Inspector.Framework.Dtos;
using Inspector.Framework.Dtos.Keycloak;
using Inspector.Framework.Dtos.Keycloak.Keycloak;
using Inspector.Framework.Helpers.Extensions;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using Prism.Logging;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zammad.Client;

namespace Inspector.Framework.Services
{
    public class AuthService : IAuthService
    {
        IKeycloakApi _keycloakApi;
        protected IPageDialogService _dialogService;
        protected ICacheService _cacheService;
        ILogger _logger;
        OAuthToken _keycloakToken;

        public AuthService(IPageDialogService dialogService, ILogger logger, ICacheService cacheService, IKeycloakApi keycloakApi)
        {
            _keycloakApi = keycloakApi;
            _logger = logger;
            _cacheService = cacheService;
            _dialogService = dialogService;

            Init();
        }

        private async void Init()
        {
            try
            {
                _keycloakToken = await _keycloakApi.Authenticate(new TokenRequestBody
                {
                    ClientId = AppKeys.KeycloakClientId,
                    GrantType = AppKeys.KeycloakGrantType,
                    Password = AppKeys.KeycloakPassword,
                    Username = AppKeys.KeycloakUsername
                });
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync("", Message.SomethingHappen, "Ok");
            }
        }

        public async Task<(bool result, bool doSignUp)> Login(string email, string password)
        {
            var response = await UserExist(email);

            if(response.exist)
            {
                var pwdList = response.keycloakUserCollection[0]?.Attributes?.Pwd;
                if (pwdList == null)
                {
                    return (false, true); // do sign up
                }

                var pwd = response.keycloakUserCollection[0]?.Attributes?.Pwd[0] ?? string.Empty;
                var result = await LoginZammad(email, pwd.Base64Decode());

                return (result, false);
            }

            return (false, false);
        }

        private async Task<(bool exist, List<KeycloakUser> keycloakUserCollection)> UserExist(string email)
        {
            var keycloakUserCollection = await _keycloakApi.GetUser($"Bearer {_keycloakToken.AccessToken}", email);
            if (keycloakUserCollection != null && keycloakUserCollection.Count == 1)
                return (true, keycloakUserCollection);

            return (false, keycloakUserCollection);
        }

        public async Task<bool> SignUp(UserRepresentation user, string pass)
        {
            var response = await UserExist(user.Email);

            if (response.exist)
            {
                //Resets the password because we are doing a "registration"
                await _keycloakApi.ResetPassword($"Bearer {_keycloakToken.AccessToken}", response.keycloakUserCollection[0].Id, new CredentialRepresentation
                {
                    Password = pass
                });

                await _keycloakApi.UpdateUser($"Bearer {_keycloakToken.AccessToken}", response.keycloakUserCollection[0].Id, user);

                var pwd = response.keycloakUserCollection[0]?.Attributes?.Pwd[0] ?? string.Empty;
                var result = await LoginZammad(user.Email, pwd.Base64Decode());

                return result;
            }

            await _keycloakApi.CreateUser($"Bearer {_keycloakToken.AccessToken}", user);
            var createdUser = await _keycloakApi.GetUser($"Bearer {_keycloakToken.AccessToken}", user.Email);

            await _keycloakApi.ResetPassword($"Bearer {_keycloakToken.AccessToken}", createdUser[0].Id, new CredentialRepresentation
            {
                Password = pass
            });

            var result2 = await LoginZammad(user.Email, pass);
            return result2;
        }


        private async Task<bool> LoginZammad(string email, string password)
        {
            try
            {
                // TODO
                // If the user exists check if the user has the cedula field
                // If the cedula field is set, see if the password match the cedula
                // If the cedula field is not set, update the user, set the cedula field, and proceed

                var account = ZammadAccount.CreateBasicAccount(AppKeys.ZammadApiBaseUrl, email, password);
                var client = account.CreateUserClient();
                var userAccount = await client.GetUserMeAsync();

                if (userAccount.Active)
                {
                    var groupClient = account.CreateGroupClient();
                    var groups = await groupClient.GetGroupListAsync();

                    await _cacheService.InsertLocalObject(CacheKeys.Groups, groups);

                    Settings.IsLoggedIn = true;
                    await _cacheService.InsertSecureObject(CacheKeys.ZammadAccount, account);
                    await _cacheService.InsertSecureObject(CacheKeys.UserAccount, userAccount);

                    return true;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("", Message.AccountNotActivated, "Ok");
                }

                return false;
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
                return false;
            }
        }

        private async Task<bool> SignUpZammad(KeycloakUser keycloakUser, UserRepresentation user)
        {
            try
            {
                // Search for the user email in zammad
                var zammadUserSearch = await _zammadLiteApi.SearchUser($"Bearer {AppKeys.ZammadToken}", email);

                var zammadGroupsMap = new Dictionary<string, List<string>>();

                foreach (var group in _groups)
                {
                    zammadGroupsMap.Add(group.Id, new List<string> { "read", "create", "change" });
                }

                zammadGroupsMap[_group.Id].Clear();
                zammadGroupsMap[_group.Id].Add("full");

                // If the user doesn't exist in zammad, create it
                if (zammadUserSearch != null && zammadUserSearch.Where(x => x.Email == email).Count() == 0)
                {
                    var zammadUser = await _zammadLiteApi.CreateUser($"Bearer {AppKeys.ZammadToken}", new ZammadUser
                    {
                        Email = email,
                        Firstname = keycloakUser.FirstName,
                        Lastname = keycloakUser.LastName,
                        Cedula = cedula,
                        Password = _password,
                        Organization = "Ogtic",
                        Note = "User created from mobile",
                        Zone = _zone,
                        Verified = true,
                        //TODO: Revaluate this assignment
                        RoleIds = new List<int>() { 2 }, //1: Admin, 2: Agent, 3: Customer
                        GroupIds = zammadGroupsMap,
                        Active = true
                    });
                }
                else
                {
                    var zammadUser = zammadUserSearch[0];
                    zammadUser.Password = password;
                    zammadUser.Zone = _zone;
                    zammadUser.Verified = true;
                    zammadUser.GroupIds = zammadGroupsMap;
                    await _zammadLiteApi.UpdateUser($"Bearer {AppKeys.ZammadToken}", zammadUser.Id, zammadUser);
                    //Update Password to document
                }

                return false;
            }
            catch (Exception e)
            {
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
                return false;
            }
        }
    }

    public interface IAuthService
    {
        Task<(bool result, bool doSignUp)> Login(string email, string password);

        Task<bool> SignUp(UserRepresentation user, string pass);
    }
}
