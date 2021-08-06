using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos.Keycloak
{
    namespace Keycloak
    {
        using System;
        using System.Collections.Generic;

        using System.Globalization;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Converters;

        public class KeycloakUser
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("createdTimestamp")]
            public long CreatedTimestamp { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("enabled")]
            public bool Enabled { get; set; }

            [JsonProperty("totp")]
            public bool Totp { get; set; }

            [JsonProperty("emailVerified")]
            public bool EmailVerified { get; set; }

            [JsonProperty("firstName")]
            public string FirstName { get; set; }

            [JsonProperty("lastName")]
            public string LastName { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("attributes")]
            public KeycloakAttributes Attributes { get; set; }

            [JsonProperty("disableableCredentialTypes")]
            public List<object> DisableableCredentialTypes { get; set; }

            [JsonProperty("requiredActions")]
            public List<object> RequiredActions { get; set; }

            [JsonProperty("notBefore")]
            public long NotBefore { get; set; }

            [JsonProperty("access")]
            public KeycloakAccess Access { get; set; }
        }


    }

}
