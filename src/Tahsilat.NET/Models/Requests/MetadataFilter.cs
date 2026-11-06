using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Requests
{
    /// <summary>
    /// Matches records that carry a metadata entry with this key and/or value.
    /// </summary>
    public class MetadataFilter
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
