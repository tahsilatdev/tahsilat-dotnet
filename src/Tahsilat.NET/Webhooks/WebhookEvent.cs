using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Webhooks
{
    public class WebhookEvent
    {
        [JsonProperty("transaction_id")]
        public long TransactionId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }

        [JsonProperty("installment_count")]
        public int InstallmentCount { get; set; }

        // Ödeme durumu
        [JsonProperty("payment_status")]
        public int PaymentStatus { get; set; }

        [JsonProperty("payment_status_text")]
        public string PaymentStatusText { get; set; }

        // İşlem durumu
        [JsonProperty("transaction_status")]
        public int TransactionStatus { get; set; }

        [JsonProperty("transaction_status_text")]
        public string TransactionStatusText { get; set; }

        [JsonProperty("transaction_message")]
        public string TransactionMessage { get; set; }

        [JsonProperty("transaction_code")]
        public string TransactionCode { get; set; }

        // Ödeme yöntemi
        [JsonProperty("payment_method")]
        public int PaymentMethod { get; set; }

        [JsonProperty("payment_method_text")]
        public string PaymentMethodText { get; set; }

        [JsonProperty("pre_auth")]
        public bool PreAuth { get; set; }

        // Tarihler
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("start_at")]
        public string StartAt { get; set; }

        [JsonProperty("end_at")]
        public string EndAt { get; set; }

        // Metadata
        [JsonProperty("metadata")]
        public List<MetadataEntry> Metadata { get; set; }
    }
}
