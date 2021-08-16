using Newtonsoft.Json;

namespace Inspector.Framework.Dtos
{
    public class Zone
    {
        [JsonProperty("identifier")]
        public string Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("regionCode")]
        public string RegionCode { get; set; }

        [JsonProperty("provinceCode")]
        public string ProvinceCode { get; set; }

        [JsonProperty("municipalityCode")]
        public string MunicipalityCode { get; set; }
    }
}
