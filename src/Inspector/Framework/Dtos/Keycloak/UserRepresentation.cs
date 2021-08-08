using Newtonsoft.Json;
using System.Collections.Generic;

namespace Inspector.Framework.Dtos.Keycloak
{
    public class UserRepresentation
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, List<string>> Attributes { get; set; }
    }
}
