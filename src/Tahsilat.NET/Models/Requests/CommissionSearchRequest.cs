using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Requests
{
    public class CommissionSearchRequest
    {
        [JsonProperty("bin_number")]
        public int? BinNumber { get; set; }
    }
}
