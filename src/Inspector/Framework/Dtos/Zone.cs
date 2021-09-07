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

        [JsonProperty("regionCode")]
        public string RegionCode { get; set; }

        [JsonProperty("provinceCode")]
        public string ProvinceCode { get; set; }

        [JsonProperty("municipalityCode")]
        public string MunicipalityCode { get; set; }

        [JsonProperty("districtCode")]
        public string DistrictCode { get; set; }

        [JsonProperty("sectionCode")]
        public string SectionCode { get; set; }

        [JsonProperty("neighborhoodCode")]
        public string NeighhborhoodCode { get; set; }

        [JsonProperty("subneighborhoodCode")]
        public string SubNeighhborhoodCode { get; set; }
    }

    public class QueryZone
    {
        [AliasAs("regionCode")]
        public string RegionCode { get; set; }

        [AliasAs("provinceCode")]
        public string ProvinceCode { get; set; }

        [AliasAs("municipalityCode")]
        public string MunicipalityCode { get; set; }

        [AliasAs("districtCode")]
        public string DistrictCode { get; set; }

        [AliasAs("sectionCode")]
        public string SectionCode { get; set; }

        [AliasAs("neighborhoodCode")]
        public string NeighhborhoodCode { get; set; }

        [AliasAs("subneighborhoodCode")]
        public string SubNeighhborhoodCode { get; set; }
    }
}
