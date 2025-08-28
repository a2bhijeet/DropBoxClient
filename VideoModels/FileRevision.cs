using Newtonsoft.Json;

namespace VideoModels
{
    public class FileRevision
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "rev")]
        public string Rev { get; set; }

        [JsonProperty(PropertyName = "size")]
        public ulong Size { get; set; }

        [JsonProperty(PropertyName = "servermodified")]
        public DateTime ServerModified { get; set; }


    }
}
