using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Common
{
    public class ProductItem
    {
        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("metadata")]
        public List<Dictionary<string, object>> Metadata { get; set; }
    }
}
