using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos.Keycloak
{
    public class CredentialRepresentation
    {
        [JsonProperty("value")]
        public string Password { get; set; }
    }
}
