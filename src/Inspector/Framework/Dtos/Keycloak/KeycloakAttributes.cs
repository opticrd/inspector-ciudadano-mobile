using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos.Keycloak
{
    public class KeycloakAttributes
    {
        [JsonProperty("cedula")]
        public List<string> Cedula { get; set; }

        [JsonProperty("pwd")]
        public List<string> Pwd { get; set; }
    }
}
