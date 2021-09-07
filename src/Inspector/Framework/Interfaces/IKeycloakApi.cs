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

        [Post("/auth/admin/realms/master/users")]
        Task<string> CreateUser([Header("Authorization")] string token, [Body(BodySerializationMethod.Serialized)] UserRepresentation user);

        [Put("/auth/admin/realms/master/users/{id}")]
        Task<string> UpdateUser([Header("Authorization")] string token, string id, [Body(BodySerializationMethod.Serialized)] UserRepresentation user);

        [Put("/auth/admin/realms/master/users/{id}/reset-password")]
        Task ResetPassword([Header("Authorization")] string token, string id, [Body(BodySerializationMethod.Serialized)] CredentialRepresentation credentialRepresentation);
    }
}
