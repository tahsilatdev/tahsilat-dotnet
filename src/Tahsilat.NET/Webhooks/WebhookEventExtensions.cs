using System;

using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Webhooks
{
    /// <summary>
    /// Extension methods for <see cref="WebhookEvent"/> to check payment and transaction statuses.
    /// </summary>
    public static class WebhookEventExtensions
    {
        #region Payment Status Checks

        /// <summary>
        /// Checks whether the payment was successful.
        /// Payment status must be Success and transaction status must be Completed or PreAuthorized.
        /// </summary>
        public static bool IsSuccess(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentStatus == (int)PaymentStatus.Success &&
                   (evt.TransactionStatus == (int)TransactionStatus.Completed ||
                    evt.TransactionStatus == (int)TransactionStatus.PreAuthorized);
        }

        /// <summary>
        /// Checks whether the payment has failed.
        /// </summary>
        public static bool IsFailed(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentStatus == (int)PaymentStatus.Failed;
        }

        /// <summary>
        /// Checks whether the payment is incomplete.
        /// </summary>
        public static bool IsIncomplete(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentStatus == (int)PaymentStatus.Incomplete;
        }

        #endregion

        #region Transaction Status Checks

        /// <summary>
        /// Checks whether the transaction is pending.
        /// </summary>
        public static bool IsPending(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Pending;
        }

        /// <summary>
        /// Checks whether the transaction is completed.
        /// </summary>
        public static bool IsCompleted(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Completed;
        }

        /// <summary>
        /// Checks whether the transaction is pre-authorized.
        /// </summary>
        public static bool IsPreAuthorized(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.PreAuthorized;
        }

        /// <summary>
        /// Checks whether the transaction is cancelled.
        /// </summary>
        public static bool IsCancelled(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Cancelled;
        }

        /// <summary>
        /// Checks whether the transaction is fully refunded.
        /// </summary>
        public static bool IsRefunded(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Refunded;
        }

        /// <summary>
        /// Checks whether the transaction is partially refunded.
        /// </summary>
        public static bool IsPartialRefunded(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.PartialRefunded;
        }

        /// <summary>
        /// Checks whether the transaction has any refund (full or partial).
        /// </summary>
        public static bool HasRefund(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Refunded ||
                   evt.TransactionStatus == (int)TransactionStatus.PartialRefunded;
        }

        /// <summary>
        /// Checks whether the transaction has a chargeback.
        /// </summary>
        public static bool IsChargeback(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Chargeback;
        }

        /// <summary>
        /// Checks whether the transaction has a partial chargeback.
        /// </summary>
        public static bool IsPartialChargeback(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.PartialChargeback;
        }

        /// <summary>
        /// Checks whether the transaction has any chargeback (full or partial).
        /// </summary>
        public static bool HasChargeback(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Chargeback ||
                   evt.TransactionStatus == (int)TransactionStatus.PartialChargeback;
        }

        /// <summary>
        /// Checks whether the transaction is flagged as fraud.
        /// </summary>
        public static bool IsFraud(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Fraud;
        }

        /// <summary>
        /// Checks whether the transaction has timed out.
        /// </summary>
        public static bool IsTimeout(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == (int)TransactionStatus.Timeout;
        }

        #endregion

        #region Payment Method Checks

        /// <summary>
        /// Checks whether the payment was made using 3D Secure.
        /// </summary>
        public static bool Is3D(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentMethod == (int)PaymentMethod.ThreeD;
        }

        /// <summary>
        /// Checks whether the payment was made using 2D (non-3DS).
        /// </summary>
        public static bool Is2D(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentMethod == (int)PaymentMethod.TwoD;
        }

        #endregion

        #region Pre-Authorization Check

        /// <summary>
        /// Checks whether this is a pre-authorization transaction.
        /// </summary>
        public static bool IsPreAuth(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PreAuth;
        }

        #endregion
    }
}
