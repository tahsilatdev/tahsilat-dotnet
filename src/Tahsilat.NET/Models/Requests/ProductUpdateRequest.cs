using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tahsilat.NET.Models.Requests
{
    public class ProductUpdateRequest
    {
        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("price")]
        public int? Price { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("metadata")]
        public List<Dictionary<string, object>> Metadata { get; set; }
    }
}
