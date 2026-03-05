namespace Tahsilat.NET.Models.Enums
{
    /// <summary>
    /// Transaction status constants matching the Tahsilat API.
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>Transaction is pending.</summary>
        Pending = 1,

        /// <summary>Transaction is completed.</summary>
        Completed = 2,

        /// <summary>Transaction is pre-authorized.</summary>
        PreAuthorized = 3,

        /// <summary>Transaction is cancelled.</summary>
        Cancelled = 4,

        /// <summary>Transaction is fully refunded.</summary>
        Refunded = 5,

        /// <summary>Transaction is partially refunded.</summary>
        PartialRefunded = 6,

        /// <summary>Transaction has a chargeback.</summary>
        Chargeback = 7,

        /// <summary>Transaction has a partial chargeback.</summary>
        PartialChargeback = 8,

        /// <summary>Transaction is flagged as fraud.</summary>
        Fraud = 9,

        /// <summary>Transaction has timed out.</summary>
        Timeout = 10
    }
}
