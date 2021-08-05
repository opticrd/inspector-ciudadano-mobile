using Inspector.Framework.Dtos;
using Inspector.Framework.Dtos.Keycloak;
using Inspector.Framework.Dtos.Keycloak.Keycloak;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Interfaces
{
    public interface IKeycloakApi
    {
        [Post("/auth/realms/master/protocol/openid-connect/token")]
        OAuthToken Authenticate([Body(BodySerializationMethod.Serialized)] TokenRequestBody tokenRequestBody);

        [Get("/auth/admin/realms/master/users")]
        KeycloakUser GetUser([Header("Authorization")]string token, string search);
    }
}
