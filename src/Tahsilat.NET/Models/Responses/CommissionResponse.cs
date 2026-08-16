using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Responses
{
    public class CommissionResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("merchant_id")]
        public long MerchantId { get; set; }

        /// <summary>
        /// Identifier of the POS credential this rate belongs to. May be null.
        /// </summary>
        [JsonProperty("company_pos_credential_id")]
        public long? CompanyPosCredentialId { get; set; }

        /// <summary>
        /// Identifier of the POS integration this rate belongs to. May be null.
        /// </summary>
        [JsonProperty("pos_id")]
        public long? PosId { get; set; }

        /// <summary>
        /// Name of the POS this rate belongs to (e.g. "Ziraat Pay Pos"). May be null.
        /// </summary>
        [JsonProperty("pos_name")]
        public string PosName { get; set; }

        [JsonProperty("installment")]
        public int Installment { get; set; }

        [JsonProperty("installment_text")]
        public string InstallmentText { get; set; }

        /// <summary>
        /// Card type the rate applies to: "credit", "debit" or "prepaid".
        /// Null means the rate applies to all card types. See <see cref="Common.CardTypes"/>.
        /// </summary>
        [JsonProperty("card_type")]
        public string CardType { get; set; }

        /// <summary>
        /// True when the rate applies to on-us transactions (card issued by the bank owning the POS),
        /// false when it applies to not-on-us only. Null means it applies to both.
        /// </summary>
        [JsonProperty("is_on_us")]
        public bool? IsOnUs { get; set; }

        /// <summary>
        /// True when the rate applies to foreign (international) cards, false for domestic cards.
        /// </summary>
        [JsonProperty("is_foreign", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsForeign { get; set; }

        [JsonProperty("commission_rate")]
        public decimal CommissionRate { get; set; }

        [JsonProperty("commission_by")]
        public int CommissionBy { get; set; }

        [JsonProperty("commission_by_text")]
        public string CommissionByText { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("card_family")]
        public CardFamilyInfo CardFamily { get; set; }

        [JsonProperty("transaction_fee")]
        public int? TransactionFee { get; set; }

        [JsonProperty("card_segment_type")]
        public CardSegmentTypeInfo CardSegmentType { get; set; }
    }

    public class CardFamilyInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("logo_url")]
        public string LogoUrl { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }

    public class CardSegmentTypeInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
