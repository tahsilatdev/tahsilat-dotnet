namespace Tahsilat.NET.Models.Enums
{
    /// <summary>
    /// Payment status constants matching the Tahsilat API.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>Payment completed successfully.</summary>
        Success = 1,

        /// <summary>Payment has failed.</summary>
        Failed = 2,

        /// <summary>Payment is incomplete.</summary>
        Incomplete = 3
    }
}
