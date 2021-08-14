using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Inspector.Framework.Dtos
{
    public class ZoneHierarchy
    {
        [JsonProperty("level")]
        public ZoneLevel Level { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("municipality")]
        public string Municipality { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("section")]
        public string Section { get; set; }

        [JsonProperty("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("subNeighborhood")]
        public string SubNeighborhood { get; set; }
    }

    public enum ZoneLevel
    {
        [EnumMember(Value = "region")]
        Region,

        [EnumMember(Value = "province")]
        Province,

        [EnumMember(Value = "municipality")]
        Municipality,

        [EnumMember(Value = "district")]
        District,

        [EnumMember(Value = "section")]
        Section,

        [EnumMember(Value = "neighborhood")]
        Neighborhood,

        [EnumMember(Value = "subNeighborhood")]
        SubNeighborhood
    }
}
