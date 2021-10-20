using Inspector.Framework.Dtos;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Inspector.Framework.Helpers
{
    /// <summary>
    /// Custom delegating handler for adding Auth headers to outbound requests
    /// </summary>
    class AuthHeaderHandler : DelegatingHandler
    {
        IAMAPI _iamClient;
        DateTime _expiresIn;
        OAuthToken _token;
        public AuthHeaderHandler(IAMAPI iamClient)
        {
            _iamClient = iamClient;
            InnerHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } //no SSL check needed yet
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
           
            if (string.IsNullOrEmpty(_token?.AccessToken) || (!string.IsNullOrEmpty(_token?.AccessToken) && _expiresIn < DateTime.Now))
            {
                _token = await _iamClient.GetToken(new Dictionary<string, object> { { "grant_type", "client_credentials" } });
                _expiresIn = DateTime.Now.AddSeconds(3600);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
            request.Headers.Add("x-access-token", AppKeys.XAccessToken);
            
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
