using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Responses
{
    public class CommissionResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("company_pos_credential_id")]
        public long CompanyPosCredentialId { get; set; }

        [JsonProperty("merchant_id")]
        public long MerchantId { get; set; }

        [JsonProperty("installment")]
        public int Installment { get; set; }

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
