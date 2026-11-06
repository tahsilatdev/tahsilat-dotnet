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
        /// Redirect URL after payment completion (optional).
        /// If left null or empty, the customer is redirected to Tahsilat's own
        /// payment result page instead of your site.
        /// Should only contain transaction_id as a query parameter.
        /// </summary>
        [JsonProperty("redirect_url")]
        public string? RedirectUrl { get; set; }

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

        /// <summary>
        /// POS definition that will collect a subscription payment. Required when
        /// the merchant has more than one subscription-capable POS (the API answers
        /// 422 otherwise); optional with a single one. Only meaningful when
        /// <see cref="ProductIds"/> contains a subscription product: a subscription
        /// cannot be started from inline <see cref="Products"/>.
        /// </summary>
        [JsonProperty("subscription_credential_id")]
        public long? SubscriptionCredentialId { get; set; }

        /// <summary>
        /// White label only: collect the card details on your own page instead of
        /// the Tahsilat payment page. Requires the white label permission on the
        /// merchant account.
        /// </summary>
        [JsonProperty("white_label")]
        public bool? WhiteLabel { get; set; }

        /// <summary>
        /// White label only: number of installments. Ignored unless
        /// <see cref="WhiteLabel"/> is true. Not allowed with subscription products.
        /// </summary>
        [JsonProperty("installment_count")]
        public int? InstallmentCount { get; set; }
    }
}
