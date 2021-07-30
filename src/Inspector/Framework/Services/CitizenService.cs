using Inspector.Framework.Dtos;
using Inspector.Framework.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Inspector.Framework.Services
{
    public class CitizenService : ICitizenAPI, IAMAPI
    {
        ICitizenAPI _citizenAPI;
        IAMAPI _iamAPI;
        OAuthToken _token;
        bool _isValidToken;

        public CitizenService(ICitizenAPI citizenAPI, IAMAPI iamAPI)
        {
            _citizenAPI = citizenAPI;
            _iamAPI = iamAPI;
        }

        public Task<Response<Citizen>> GetCitizenBasicInfo(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<Response<Citizen>> GetCitizenBirthInfo(string id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<OAuthToken> GetToken()
        {
            _token = await _iamAPI.GetToken();
            var timer = new Timer(x => _isValidToken = false);

            return _token;
        }
    }
}
