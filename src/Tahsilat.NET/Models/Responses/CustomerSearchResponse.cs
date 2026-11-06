using Newtonsoft.Json;
using System.Collections.Generic;
using Tahsilat.NET.Models.Common;

namespace Tahsilat.NET.Models.Responses
{
    /// <summary>
    /// One page of customer search results: <c>{ "meta": {...}, "customers": [...] }</c>.
    /// </summary>
    public class CustomerSearchResponse
    {
        [JsonProperty("meta")]
        public SearchMeta Meta { get; set; }

        [JsonProperty("customers")]
        public List<CustomerResponse> Customers { get; set; }
    }
}
