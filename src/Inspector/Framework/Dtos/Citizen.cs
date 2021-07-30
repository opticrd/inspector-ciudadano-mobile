using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Inspector.Framework.Dtos
{
    public class Citizen
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("names")]
        public string Names { get; set; }

        [JsonProperty("firstSurname")]
        public string FirstSurname { get; set; }

        [JsonProperty("secondSurname")]
        public string SecondSurname { get; set; }

        [JsonProperty("gender")]
        public Gender Gender { get; set; }



        [JsonProperty("birthPlace")]
        public string BirthPlace { get; set; }

        [JsonProperty("birthDate")]
        public DateTime BirthDate { get; set; }

        [JsonProperty("nationality")]
        public string Nationality { get; set; }
    }

    public enum Gender
    {
        [EnumMember(Value = "M")]
        Male,

        [EnumMember(Value = "F")]
        Female
    }
}
