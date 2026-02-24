using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Models.Requests
{
    public class PaymentCreateRequest
    {
        /// <summary>
        /// Ödeme ile ilişkilendirilecek müşteri ID’si (opsiyonel)
        /// </summary>
        [JsonProperty("customer_id")]
        public long? CustomerId { get; set; }

        /// <summary>
        /// Ödeme tutarı (kuruş cinsinden)
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// ISO 4217 para birimi kodu (örn: "TRY") — required
        /// </summary>
        [JsonProperty("currency")]
        public string? Currency { get; set; }

        /// <summary>
        /// 3D ödeme sonrası yönlendirilecek URL (opsiyonel)
        /// Query parametresi olarak sadece transaction_id içermeli
        /// </summary>
        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Ön provizyon işlemi true ise ödeme sadece blokeye alınır 
        /// Varsayılan false (normal ödeme)
        /// </summary>
        [JsonProperty("pre_auth")]
        public bool PreAuth { get; set; } = false;

        /// <summary>
        /// Inline ürün listesi — ürün ismi, fiyatı, açıklaması
        /// Eğer product_ids gönderilmiyorsa bu zorunludur.
        /// </summary>
        [JsonProperty("products")]
        public List<ProductItem>? Products { get; set; }

        /// <summary>
        /// Önceden tanımlı ürün ID listesi — Eğer products YOKSA zorunlu
        /// </summary>
        [JsonProperty("product_ids")]
        public List<long>? ProductIds { get; set; }

        /// <summary>
        /// Ek metadata — her türlü özel rapor bilgisi
        /// </summary>
        [JsonProperty("metadata")]
        public List<Dictionary<string, object>>? Metadata { get; set; }

        /// <summary>
        /// Ödeme ile ilgili yazılacak açıklama bilgisi (opsiyonel)
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }
    }
}
