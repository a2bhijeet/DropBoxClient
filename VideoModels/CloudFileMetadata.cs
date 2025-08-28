using Newtonsoft.Json;

namespace VideoBackend.Model
{
    public class CloudFileMetadata
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "size")]
        public ulong Size { get; set; }

        [JsonProperty(PropertyName = "pathdisplay")]
        public string PathDisplay { get; set; }

        [JsonProperty(PropertyName = "servermodified")]
        public DateTime ServerModified { get; set; }
    }
}
