using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;

namespace Tahsilat.NET.Models.Responses
{
    public class ProductResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("merchant_id")]
        public long MerchantId { get; set; }

        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("product_image")]
        public string ProductImage { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("formatted_price")]
        public string FormattedPrice { get; set; }

        [JsonProperty("formatted_created_at")]
        public string FormattedCreatedAt { get; set; }

        [JsonProperty("system_id")]
        public int SystemId { get; set; }

        [JsonProperty("metadata")]
        public List<MetadataEntry> Metadata { get; set; }

        [JsonProperty("timeline")]
        public List<TimelineEntry> Timeline { get; set; }
    }
}
