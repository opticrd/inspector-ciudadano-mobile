using Refit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos.Keycloak
{
    public class TokenRequestBody
    {
        [AliasAs("username")]
        public string Username { get; set; }

        [AliasAs("password")]
        public string Password { get; set; }

        [AliasAs("client_id")]
        public string ClientId { get; set; }

        [AliasAs("grant_type")]
        public string GrantType { get; set; }
    }
}
