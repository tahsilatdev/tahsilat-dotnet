using Newtonsoft.Json;
using System.Collections.Generic;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Models.Requests
{
    /// <summary>
    /// Fields to change on a product; null properties are left untouched.
    /// While a subscription linked to the product is Active or Pending, the name,
    /// price and subscription fields cannot be changed and the API answers 409.
    /// </summary>
    public class ProductUpdateRequest
    {
        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        /// <summary>
        /// Price in minor units (kuruş).
        /// </summary>
        [JsonProperty("price")]
        public int? Price { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Up to 25 entries. Key: ^[a-z][a-z0-9_]*$, max 20 chars; value: max 200 chars.
        /// </summary>
        [JsonProperty("metadata")]
        public List<Dictionary<string, object>> Metadata { get; set; }

        /// <summary>
        /// Marks the product as a subscription (recurring) product. When true,
        /// <see cref="BillingInterval"/> and <see cref="IntervalCount"/> are required.
        /// </summary>
        [JsonProperty("is_recurring")]
        public bool? IsRecurring { get; set; }

        /// <summary>
        /// Billing period unit. Required when <see cref="IsRecurring"/> is true.
        /// </summary>
        [JsonProperty("billing_interval")]
        public BillingInterval? BillingInterval { get; set; }

        /// <summary>
        /// Multiplier for <see cref="BillingInterval"/>, 1-366. Required when
        /// <see cref="IsRecurring"/> is true.
        /// </summary>
        [JsonProperty("interval_count")]
        public int? IntervalCount { get; set; }
    }
}
