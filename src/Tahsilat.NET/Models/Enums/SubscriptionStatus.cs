namespace Tahsilat.NET.Models.Enums
{
    /// <summary>
    /// Subscription status constants matching the Tahsilat API
    /// (<c>subscription.status</c> in transaction results and webhooks).
    /// </summary>
    public enum SubscriptionStatus
    {
        /// <summary>Running; the next charge happens on next_billing_at.</summary>
        Active = 1,

        /// <summary>The latest charge failed and is being retried.</summary>
        PastDue = 2,

        /// <summary>Cancelled by the merchant or the customer; no further charges.</summary>
        Cancelled = 3,

        /// <summary>Every planned cycle has been collected.</summary>
        Completed = 4,

        /// <summary>Created but the initial 3D Secure payment has not been made yet.</summary>
        Pending = 5
    }
}
