using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Models.Requests
{
    public class PaymentCreateRequest
    {
        /// <summary>
        /// Customer ID to associate with the payment (optional)
        /// </summary>
        [JsonProperty("customer_id")]
        public long? CustomerId { get; set; }

        /// <summary>
        /// Payment amount (in minor units, e.g. kurus)
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// ISO 4217 currency code (e.g. "TRY") — required
        /// </summary>
        [JsonProperty("currency")]
        public string? Currency { get; set; }

        /// <summary>
        /// Redirect URL after 3D payment completion (optional).
        /// Should only contain transaction_id as a query parameter.
        /// </summary>
        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// If true, the payment is only authorized (pre-auth), not captured.
        /// Default is false (normal payment).
        /// </summary>
        [JsonProperty("pre_auth")]
        public bool PreAuth { get; set; } = false;

        /// <summary>
        /// Inline product list — product name, price, and description.
        /// Required if product_ids is not provided.
        /// </summary>
        [JsonProperty("products")]
        public List<ProductItem>? Products { get; set; }

        /// <summary>
        /// List of pre-defined product IDs — required if products is not provided.
        /// </summary>
        [JsonProperty("product_ids")]
        public List<long>? ProductIds { get; set; }

        /// <summary>
        /// Additional metadata — custom reporting data.
        /// </summary>
        [JsonProperty("metadata")]
        public List<Dictionary<string, object>>? Metadata { get; set; }

        /// <summary>
        /// Description for the payment (optional).
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }
    }
}
