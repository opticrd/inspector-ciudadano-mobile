using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos.Zammad
{
    public class ZammadGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("signature_id")]
        public string SignatureId { get; set; }

        [JsonProperty("email_address_id")]
        public string EmailAddressId { get; set; }

        [JsonProperty("assignment_timeout")]
        public string AssignmentTimeout { get; set; }

        [JsonProperty("follow_up_possible")]
        public string FollowUpPossible { get; set; }

        [JsonProperty("follow_up_assignment")]
        public bool FollowUpAssignment { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
