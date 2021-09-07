using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos.Keycloak
{
    public class KeycloakAccess
    {
        [JsonProperty("manageGroupMembership")]
        public bool ManageGroupMembership { get; set; }

        [JsonProperty("view")]
        public bool View { get; set; }

        [JsonProperty("mapRoles")]
        public bool MapRoles { get; set; }

        [JsonProperty("impersonate")]
        public bool Impersonate { get; set; }

        [JsonProperty("manage")]
        public bool Manage { get; set; }
    }

}
