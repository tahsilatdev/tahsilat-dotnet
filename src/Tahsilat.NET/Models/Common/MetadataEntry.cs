using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// One key/value pair of a record's metadata as returned by the API.
    /// </summary>
    public class MetadataEntry
    {
        [JsonProperty("key")]
        public string Key { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string? Value { get; set; }
    }
}
