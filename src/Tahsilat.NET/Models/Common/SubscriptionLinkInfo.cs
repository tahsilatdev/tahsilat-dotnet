using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// The <c>subscription</c> block of a product or customer (GET by id only):
    /// whether a subscription is linked to the record. Any further fields the API
    /// adds for linked subscriptions are kept in <see cref="Extra"/>.
    /// Not to be confused with <see cref="SubscriptionInfo"/>, the block on a
    /// transaction result or webhook.
    /// </summary>
    public class SubscriptionLinkInfo
    {
        [JsonProperty("has_subscription")]
        public bool HasSubscription { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
