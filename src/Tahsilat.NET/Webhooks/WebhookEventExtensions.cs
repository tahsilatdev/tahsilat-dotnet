using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tahsilat.NET.Webhooks
{
    public static class WebhookEventExtensions
    {
        #region Payment Status Checks

        /// <summary>
        /// Checks whether the payment was successful.
        /// PHP SDK: $event->isSuccess()
        /// </summary>
        public static bool IsSuccess(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentStatus == 1; // PaymentStatus.Success
        }

        /// <summary>
        /// Checks whether the payment has failed.
        /// PHP SDK: $event->isFailed()
        /// </summary>
        public static bool IsFailed(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentStatus == 2; // PaymentStatus.Failed
        }

        /// <summary>
        /// Checks whether the payment is pending.
        /// </summary>
        public static bool IsPending(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentStatus == 3; // PaymentStatus.Pending
        }

        #endregion

        #region Transaction Status Checks

        /// <summary>
        /// Checks whether the transaction is completed.
        /// </summary>
        public static bool IsCompleted(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == 2; // TransactionStatus.Completed
        }

        /// <summary>
        /// Checks whether the transaction is cancelled.
        /// </summary>
        public static bool IsCancelled(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == 4; // TransactionStatus.Cancelled
        }

        /// <summary>
        /// Checks whether the transaction has been refunded (full or partial).
        /// </summary>
        public static bool IsRefunded(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.TransactionStatus == 5 || evt.TransactionStatus == 6; // Refunded or PartialRefund
        }

        #endregion

        #region Payment Method Checks

        /// <summary>
        /// Checks whether the payment was made using 3D Secure.
        /// PHP SDK: payment_method_text == "is_3d"
        /// </summary>
        public static bool Is3D(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentMethod == 1; // PaymentMethod.Is3D
        }

        /// <summary>
        /// Checks whether the payment was made using 2D (non-3DS).
        /// PHP SDK: payment_method_text == "is_2d"
        /// </summary>
        public static bool Is2D(this WebhookEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            return evt.PaymentMethod == 2; // PaymentMethod.Is2D
        }

        #endregion

    }
}
