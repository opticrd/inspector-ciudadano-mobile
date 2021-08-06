using Inspector.Framework.Dtos;
using Inspector.Framework.Dtos.Keycloak;
using Inspector.Framework.Dtos.Keycloak.Keycloak;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Framework.Interfaces
{
    public interface IKeycloakApi
    {
        [Post("/auth/realms/master/protocol/openid-connect/token")]
        Task<OAuthToken> Authenticate([Body(BodySerializationMethod.UrlEncoded)] TokenRequestBody tokenRequestBody);

        [Get("/auth/admin/realms/master/users")]
        Task<List<KeycloakUser>> GetUser([Header("Authorization")]string token, string search);

        [Get("/auth/admin/realms/master/users")]
        Task<List<KeycloakUser>> CreateUser([Header("Authorization")] string token, [Body] KeycloakUser user);
    }
}
