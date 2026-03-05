using System;
using Tahsilat.NET.Models.Enums;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="TransactionResult"/> to check payment and transaction statuses.
    /// </summary>
    public static class TransactionResultExtensions
    {
        #region Payment Status Checks

        /// <summary>
        /// Checks whether the payment was successful.
        /// Payment status must be Success and transaction status must be Completed or PreAuthorized.
        /// </summary>
        public static bool IsSuccess(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.PaymentStatus == (int)PaymentStatus.Success &&
                   (result.TransactionStatus == (int)TransactionStatus.Completed ||
                    result.TransactionStatus == (int)TransactionStatus.PreAuthorized);
        }

        /// <summary>
        /// Checks whether the payment has failed.
        /// </summary>
        public static bool IsFailed(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.PaymentStatus == (int)PaymentStatus.Failed;
        }

        /// <summary>
        /// Checks whether the payment is incomplete.
        /// </summary>
        public static bool IsIncomplete(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.PaymentStatus == (int)PaymentStatus.Incomplete;
        }

        #endregion

        #region Transaction Status Checks

        /// <summary>
        /// Checks whether the transaction is pending.
        /// </summary>
        public static bool IsPending(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Pending;
        }

        /// <summary>
        /// Checks whether the transaction is completed.
        /// </summary>
        public static bool IsCompleted(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Completed;
        }

        /// <summary>
        /// Checks whether the transaction is pre-authorized.
        /// </summary>
        public static bool IsPreAuthorized(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.PreAuthorized;
        }

        /// <summary>
        /// Checks whether the transaction is cancelled.
        /// </summary>
        public static bool IsCancelled(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Cancelled;
        }

        /// <summary>
        /// Checks whether the transaction is fully refunded.
        /// </summary>
        public static bool IsRefunded(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Refunded;
        }

        /// <summary>
        /// Checks whether the transaction is partially refunded.
        /// </summary>
        public static bool IsPartialRefunded(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.PartialRefunded;
        }

        /// <summary>
        /// Checks whether the transaction has any refund (full or partial).
        /// </summary>
        public static bool HasRefund(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Refunded ||
                   result.TransactionStatus == (int)TransactionStatus.PartialRefunded;
        }

        /// <summary>
        /// Checks whether the transaction has a chargeback.
        /// </summary>
        public static bool IsChargeback(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Chargeback;
        }

        /// <summary>
        /// Checks whether the transaction has a partial chargeback.
        /// </summary>
        public static bool IsPartialChargeback(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.PartialChargeback;
        }

        /// <summary>
        /// Checks whether the transaction has any chargeback (full or partial).
        /// </summary>
        public static bool HasChargeback(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Chargeback ||
                   result.TransactionStatus == (int)TransactionStatus.PartialChargeback;
        }

        /// <summary>
        /// Checks whether the transaction is flagged as fraud.
        /// </summary>
        public static bool IsFraud(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Fraud;
        }

        /// <summary>
        /// Checks whether the transaction has timed out.
        /// </summary>
        public static bool IsTimeout(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.TransactionStatus == (int)TransactionStatus.Timeout;
        }

        #endregion

        #region Payment Method Checks

        /// <summary>
        /// Checks whether the payment was made using 3D Secure.
        /// </summary>
        public static bool Is3D(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.PaymentMethod == (int)PaymentMethod.ThreeD;
        }

        /// <summary>
        /// Checks whether the payment was made using 2D (non-3DS).
        /// </summary>
        public static bool Is2D(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.PaymentMethod == (int)PaymentMethod.TwoD;
        }

        #endregion

        #region Pre-Authorization Check

        /// <summary>
        /// Checks whether this is a pre-authorization transaction.
        /// </summary>
        public static bool IsPreAuth(this TransactionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.PreAuth;
        }

        #endregion
    }
}
