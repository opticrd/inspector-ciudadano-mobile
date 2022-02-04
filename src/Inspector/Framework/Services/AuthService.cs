using Inspector.Framework.Dtos;
using Inspector.Framework.Dtos.Keycloak;
using Inspector.Framework.Dtos.Keycloak.Keycloak;
using Inspector.Framework.Dtos.Zammad;
using Inspector.Framework.Helpers.Extensions;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using Microsoft.AppCenter.Crashes;
using Prism.Logging;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.Framework.Services
{
    public class AuthService : IAuthService
    {
        IKeycloakApi _keycloakApi;
        protected IPageDialogService _dialogService;
        protected ICacheService _cacheService;
        ILogger _logger;
        //OAuthToken _keycloakToken;
        UserClient _userClient;
        GroupClient _groupClient;
        OrganizationClient _organizationClient;

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
                //_keycloakToken = await _keycloakApi.Authenticate(new TokenRequestBody
                //{
                //    ClientId = AppKeys.KeycloakClientId,
                //    GrantType = AppKeys.KeycloakGrantType,
                //    Password = AppKeys.KeycloakPassword,
                //    Username = AppKeys.KeycloakUsername
                //});

                var account = ZammadAccount.CreateTokenAccount(AppKeys.ZammadApiBaseUrl, AppKeys.ZammadToken);
                _userClient = account.CreateUserClient();
                _groupClient = account.CreateGroupClient();
                _organizationClient = account.CreateOrganizationClient();
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync("", Message.SomethingHappen, "Ok");
            }
        }

        public async Task<(bool result, bool doSignUp)> Login(string email)
        {
            var response = await UserExist(email);

            if(response.exist)
            {
                //var pwdList = response.keycloakUserCollection[0]?.Attributes?.Pwd;
                //if (pwdList == null)
                //{
                //    return (false, true); // do sign up
                //}

                //var pwd = response.keycloakUserCollection[0]?.Attributes?.Pwd[0]?.Base64Decode() ?? string.Empty;

                var user = response.user;
                if (user == null || !user.Active)
                    return (false, false);

                var password = user.Password ?? (string)user.CustomAttributes["cedula"];
                var result = await LoginZammad(email, password);

                _logger.TrackEvent("AuthLoginProcessCompleted", new Dictionary<string, string>
                {
                    //{ "KeyCloackPasswordGetIt", string.IsNullOrEmpty(pwd).ToString() },
                    { "ZammadProcess", result.ToString() },
                });

                return (result, false);
            }

            return (false, true); // do sign up
        }

        public async Task<(bool result, bool doLogin)> SignUp(ZammadUser user)
        {
            var response = await UserExist(user.Email);

            if (response.exist)
            {
                ////Resets the password because we are doing a "registration"
                //await _keycloakApi.ResetPassword($"Bearer {_keycloakToken.AccessToken}", response.keycloakUserCollection[0].Id, new CredentialRepresentation
                //{
                //    Password = password
                //});

                //await _keycloakApi.UpdateUser($"Bearer {_keycloakToken.AccessToken}", response.keycloakUserCollection[0].Id, user);

                //var pwd = response.keycloakUserCollection[0]?.Attributes?.Pwd[0] ?? string.Empty;
                //var result = await LoginZammad(user.Email, pwd.Base64Decode());

                return (false, true); // do login
            }

            //var token = await GetKeyCloakToken();
            //var createdUser = await _keycloakApi.GetUser(token, user.Email);

            //if (createdUser?.Count == 0)
            //{
            //    var newKeycloakUser = new UserRepresentation
            //    {
            //        FirstName = user.Firstname,
            //        LastName = user.Lastname,
            //        EmailVerified = true,
            //        Enabled = true,
            //        Username = user.Email,
            //        Email = user.Email,
            //        Attributes = new Dictionary<string, List<string>> 
            //        {
            //            { "cedula", new List<string> { user.Cedula } },
            //            { "pwd", new List<string> { user.Password.Base64Encode() } },
            //        },                    
            //    };

            //    var resp = await _keycloakApi.CreateUser(token, newKeycloakUser);

            //    if(resp.StatusCode != System.Net.HttpStatusCode.Created)
            //        return (false, false);

            //    createdUser = await _keycloakApi.GetUser(token, user.Email);

            //    if (createdUser?.Count == 0)
            //        return (false, false);
            //}

            //await _keycloakApi.ResetPassword(token, createdUser[0].Id, new CredentialRepresentation
            //{
            //    Password = user.Password
            //});

            var zammadResponse = await SignUpZammad(user);
            _logger.TrackEvent("AuthSignUpProcessCompleted", new Dictionary<string, string>
            {
                //{ "KeyCloackUserCreated", (createdUser?.Count == 0).ToString() },
                { "ZammadProcess", zammadResponse.ToString() },
            });

            return (zammadResponse, false);
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
                    //var groupClient = account.CreateGroupClient();
                    //var groups = await groupClient.GetGroupListAsync();

                    //await _cacheService.InsertLocalObject(CacheKeys.Groups, groups);

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
                Crashes.TrackError(e);
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
                return false;
            }
        }

        private async Task<bool> SignUpZammad(ZammadUser user)
        {
            try
            {
                var groups = await _groupClient.GetGroupListAsync();
                var orgs = await _organizationClient.SearchOrganizationAsync("Ogtic", 1);

                var zammadGroupsMap = new Dictionary<int, List<string>>();

                foreach (var group in groups)                
                    zammadGroupsMap.Add(group.Id, new List<string> { "read", "create", "change" });

                //zammadGroupsMap[_group.Id].Clear();
                //zammadGroupsMap[_group.Id].Add("full");

                var userZammad = await UserExistInZammad(user.Email);

                if (!userZammad.exist)
                {
                    var newUser = new User
                    {
                        Email = user.Email,
                        FirstName = user.Firstname,
                        LastName = user.Lastname,
                        //OrganizationId = orgs[0].Id,
                        Organization = user.Organization,
                        Note = "User created from mobile app",
                        Verified = true,
                        RoleIds = new List<int>() { 2 }, //1: Admin, 2: Agent, 3: Customer //TODO: Revaluate this assignment
                        GroupIds = zammadGroupsMap,
                        Active = true,
                        Password = user.Cedula,
                        CustomAttributes = new Dictionary<string, object>()
                        {
                            { "cedula",  user.Cedula },
                            { "zone",  user.Zone },
                        }
                    };

                    var reponse = await _userClient.CreateUserAsync(newUser);

                    if (reponse != null)
                        return true;
                }
                else
                {
                    var zammadUser = userZammad.user;
                    zammadUser.Password = user.Password;
                    zammadUser.Verified = true;
                    zammadUser.GroupIds = zammadGroupsMap;
                    zammadUser.CustomAttributes = new Dictionary<string, object>()
                    {
                        { "zone",  user.Zone },
                    };

                    var updatedUser = await _userClient.UpdateUserAsync(zammadUser.Id, zammadUser);

                    if (updatedUser != null)
                        return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync("", Message.AccountInvalid, "Ok");
                return false;
            }
        }

        public async Task<(bool exist, /*List<KeycloakUser> keycloakUserCollection,*/ User user)> UserExist(string parameter, SearchParameter type = SearchParameter.Email)
        {
            //List<KeycloakUser> keycloakUserCollection = null;

            //if (type == SearchParameter.Email)
            //{
            //    keycloakUserCollection = await _keycloakApi.GetUser(await GetKeyCloakToken(), parameter);

            //    if (keycloakUserCollection == null || keycloakUserCollection.Count == 0)
            //        return (false, null, null);
            //}

            var userZammad = await UserExistInZammad(parameter);

            if (userZammad.exist)
                return (true, userZammad.user);

            return (false, null);
        }
        private async Task<(bool exist, User user)> UserExistInZammad(string parameter)
        {
            var zammadUserSearch = await _userClient.SearchUserAsync(parameter, 1);

            if (zammadUserSearch?.Count() > 0 && zammadUserSearch[0].Active)
                return (true, zammadUserSearch[0]);

            return (false, null);
        }

        //private async Task<OAuthToken> AuthKeyCloak()
        //{
        //    var auth = await _keycloakApi.Authenticate(new TokenRequestBody
        //    {
        //        ClientId = AppKeys.KeycloakClientId,
        //        GrantType = AppKeys.KeycloakGrantType,
        //        Password = AppKeys.KeycloakPassword,
        //        Username = AppKeys.KeycloakUsername
        //    });

        //    auth.AccessToken = $"Bearer {auth.AccessToken}";

        //    return auth;
        //}

        //private async Task<string> GetKeyCloakToken()
        //{
        //    var auth = await AuthKeyCloak();

        //    return auth.AccessToken;
        //}

    }

    public enum SearchParameter
    {
        Id,
        Email
    }

    public interface IAuthService
    {
        Task<(bool result, bool doSignUp)> Login(string email);

        Task<(bool result, bool doLogin)> SignUp(ZammadUser user);

        Task<(bool exist, /*List<KeycloakUser> keycloakUserCollection,*/ User user)> UserExist(string parameter, SearchParameter type = SearchParameter.Email);
    }
}
