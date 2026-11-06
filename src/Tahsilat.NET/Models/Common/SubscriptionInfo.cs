using Newtonsoft.Json;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// The <c>subscription</c> block of a transaction result or webhook event.
    /// Present only when the transaction belongs to a subscription; null otherwise.
    /// </summary>
    public class SubscriptionInfo
    {
        [JsonProperty("subscription_id")]
        public long SubscriptionId { get; set; }

        /// <summary>
        /// Status at the time of the query, not at the time of the event. See
        /// <see cref="SubscriptionStatus"/>.
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// Status as text: "active", "past_due", "cancelled", "completed", "pending".
        /// </summary>
        [JsonProperty("status_text")]
        public string StatusText { get; set; }

        /// <summary>
        /// Billing cycle this transaction belongs to. 0 is the initial 3D Secure
        /// payment; retries within the same cycle share the same number.
        /// </summary>
        [JsonProperty("cycle_number")]
        public int CycleNumber { get; set; }

        /// <summary>
        /// Planned date of the next charge (ISO 8601). Not updated after
        /// cancellation, so ignore it when <see cref="CancelledAt"/> is set.
        /// </summary>
        [JsonProperty("next_billing_at")]
        public string NextBillingAt { get; set; }

        /// <summary>
        /// Cancellation date (ISO 8601), or null while the subscription is not cancelled.
        /// </summary>
        [JsonProperty("cancelled_at")]
        public string CancelledAt { get; set; }

        /// <summary>
        /// True for the initial 3D Secure payment that created the subscription.
        /// </summary>
        [JsonProperty("is_initial_payment")]
        public bool IsInitialPayment { get; set; }

        /// <summary>
        /// Turkish label for <see cref="Status"/>: "Aktif", "Ödeme Bekliyor",
        /// "İptal Edildi", "Tamamlandı", "İlk Ödeme Bekleniyor"; "Bilinmiyor" for
        /// a value the SDK does not know.
        /// </summary>
        public string GetStatusTextTr()
        {
            switch ((SubscriptionStatus)Status)
            {
                case SubscriptionStatus.Active: return "Aktif";
                case SubscriptionStatus.PastDue: return "Ödeme Bekliyor";
                case SubscriptionStatus.Cancelled: return "İptal Edildi";
                case SubscriptionStatus.Completed: return "Tamamlandı";
                case SubscriptionStatus.Pending: return "İlk Ödeme Bekleniyor";
                default: return "Bilinmiyor";
            }
        }
    }
}
