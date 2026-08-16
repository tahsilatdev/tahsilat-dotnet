using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Responses
{
    /// <summary>
    /// Refund record shape.
    /// NOTE: POST /transaction/refund does not populate the "data" field — the
    /// Data property of its ApiResponse is always null. Read the outcome from
    /// Status and Message instead, and re-query the transaction with
    /// Transactions.RetrieveAsync to see the refund reflected on it.
    /// This type is kept so the response shape stays forward compatible.
    /// </summary>
    public class RefundResponse
    {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("payment_transaction_id")]
        public int? PaymentTransactionId { get; set; }

        [JsonProperty("merchant_id")]
        public int? MerchantId { get; set; }

        [JsonProperty("company_pos_credential_id")]
        public int? CompanyPosCredentialId { get; set; }

        [JsonProperty("prev_payment_status")]
        public int? PrevPaymentStatus { get; set; }

        [JsonProperty("prev_transaction_status")]
        public int? PrevTransactionStatus { get; set; }

        [JsonProperty("refund_amount")]
        public int? RefundAmount { get; set; }

        [JsonProperty("formatted_refund_amount")]
        public string FormattedRefundAmount { get; set; }

        [JsonProperty("commission_rate")]
        public double? CommissionRate { get; set; }

        [JsonProperty("company_refund_loss")]
        public int? CompanyRefundLoss { get; set; }

        [JsonProperty("platform_profit")]
        public int? PlatformProfit { get; set; }

        [JsonProperty("refund_charged_amount")]
        public int? RefundChargedAmount { get; set; }

        [JsonProperty("status")]
        public int? Status { get; set; }

        [JsonProperty("status_text")]
        public string StatusText { get; set; }

        [JsonProperty("reject_reason")]
        public string RejectReason { get; set; }

        [JsonProperty("refund_type")]
        public int? RefundType { get; set; }

        [JsonProperty("refund_type_text")]
        public string RefundTypeText { get; set; }

        [JsonProperty("reference_code")]
        public string ReferenceCode { get; set; }

        [JsonProperty("bank_response_code")]
        public string BankResponseCode { get; set; }

        [JsonProperty("bank_response_message")]
        public string BankResponseMessage { get; set; }

        [JsonProperty("bank_response_receive_at")]
        public string BankResponseReceiveAt { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("formatted_refund_date")]
        public string FormattedRefundDate { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }
    }
}
