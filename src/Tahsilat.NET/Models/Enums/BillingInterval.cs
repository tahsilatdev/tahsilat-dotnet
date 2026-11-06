namespace Tahsilat.NET.Models.Enums
{
    /// <summary>
    /// Billing period unit of a subscription product (<c>billing_interval</c>).
    /// Combined with <c>interval_count</c>: Month + 2 bills every two months.
    /// </summary>
    public enum BillingInterval
    {
        Day = 1,
        Week = 2,
        Month = 3,
        Year = 4
    }
}
