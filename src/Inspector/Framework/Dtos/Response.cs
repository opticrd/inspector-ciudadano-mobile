using Newtonsoft.Json;

namespace Inspector.Framework.Dtos
{
    public class Response<T>
    {
        [JsonProperty("valid")]
        public bool Valid { get; set; }

        [JsonProperty("payload")]
        public T Payload { get; set; }
    }
}
