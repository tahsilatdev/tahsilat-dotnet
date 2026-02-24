using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Requests
{
    public class RefundCreateRequest
    {
        [JsonProperty("transaction_id")]
        public long TransactionId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
