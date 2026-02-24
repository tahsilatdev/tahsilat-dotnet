using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Responses
{
    public class PaymentResponse
    {
        [JsonProperty("transaction_id")]
        public long TransactionId { get; set; }

        [JsonProperty("payment_page_url")]
        public string PaymentPageUrl { get; set; }

        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }
    }
}
