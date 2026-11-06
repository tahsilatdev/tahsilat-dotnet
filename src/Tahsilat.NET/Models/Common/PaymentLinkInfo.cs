using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// The <c>payment_link</c> block of a customer or product. The API always
    /// sends <c>has_payment_link</c>; the link's own fields are only present when
    /// it is true and are kept in <see cref="Extra"/> until their shape is
    /// confirmed against a record that has one.
    /// </summary>
    public class PaymentLinkInfo
    {
        [JsonProperty("has_payment_link")]
        public bool HasPaymentLink { get; set; }

        /// <summary>
        /// Every other field of the block (e.g. the link URL), keyed by its JSON name.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
