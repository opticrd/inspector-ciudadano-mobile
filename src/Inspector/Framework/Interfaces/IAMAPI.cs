using Inspector.Framework.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Framework.Interfaces
{
    //[Headers("Authorization: Basic")]
    public interface IAMAPI
    {
        [Post("/oauth/token")]
        Task<OAuthToken> GetToken([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);
    }
}
