using Newtonsoft.Json;
using System.Collections.Generic;
using Tahsilat.NET.Models.Common;

namespace Tahsilat.NET.Models.Responses
{
    /// <summary>
    /// One page of product search results: <c>{ "meta": {...}, "products": [...] }</c>.
    /// </summary>
    public class ProductSearchResponse
    {
        [JsonProperty("meta")]
        public SearchMeta Meta { get; set; }

        [JsonProperty("products")]
        public List<ProductResponse> Products { get; set; }
    }
}
