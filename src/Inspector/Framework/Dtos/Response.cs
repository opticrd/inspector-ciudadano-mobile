using Inspector.Framework.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Inspector.Framework.Dtos
{
    public class Response<T>
    {
        [JsonProperty("valid")]
        public bool Valid { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("payload")]
        public T Payload { get; set; }
    }

    public class ResponseZone
    {
        [JsonProperty("valid")]
        public bool Valid { get; set; }

        [JsonProperty("data")]
        [JsonConverter(typeof(SingleOrArrayConverter<Zone>))]
        public List<Zone> Data { get; set; }
    }
}
