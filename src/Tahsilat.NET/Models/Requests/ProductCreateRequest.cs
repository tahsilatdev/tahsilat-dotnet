using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Requests
{
    public class ProductCreateRequest
    {
        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("stock_code")]
        public string StockCode { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("metadata")]
        public List<Dictionary<string, object>> Metadata { get; set; }
    }
}
