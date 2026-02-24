using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Requests
{
    public class PreAuthResolveRequest
    {
        [JsonProperty("transaction_id")]
        public long TransactionId { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }
    }
}
