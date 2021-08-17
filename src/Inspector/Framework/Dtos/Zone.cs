using Newtonsoft.Json;
using Refit;

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

        [AliasAs("regionCode")]
        [JsonProperty("regionCode")]
        public string RegionCode { get; set; }

        [AliasAs("provinceCode")]
        [JsonProperty("provinceCode")]
        public string ProvinceCode { get; set; }

        [AliasAs("municipalityCode")]
        [JsonProperty("municipalityCode")]
        public string MunicipalityCode { get; set; }
    }
}
