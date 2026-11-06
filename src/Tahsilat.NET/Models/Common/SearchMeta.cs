using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// Pagination block (<c>data.meta</c>) returned by the search endpoints, e.g.
    /// <c>{ "total": 1, "limit": 2, "page": 1, "total_page": 1, "sort": null, "duration": null }</c>.
    /// Properties are nullable because the API omits or nulls the ones that do
    /// not apply. Fields the SDK does not model are kept in <see cref="Extra"/>.
    /// </summary>
    public class SearchMeta
    {
        /// <summary>Total number of rows matching the filters.</summary>
        [JsonProperty("total")]
        public int? Total { get; set; }

        /// <summary>Rows per page that was applied.</summary>
        [JsonProperty("limit")]
        public int? Limit { get; set; }

        /// <summary>Current 1-based page.</summary>
        [JsonProperty("page")]
        public int? Page { get; set; }

        /// <summary>Number of pages for the applied limit.</summary>
        [JsonProperty("total_page")]
        public int? TotalPage { get; set; }

        /// <summary>Zero-based offset of the first row, when paging by start.</summary>
        [JsonProperty("start")]
        public int? Start { get; set; }

        /// <summary>Sort applied by the API, when it reports one.</summary>
        [JsonProperty("sort")]
        public string Sort { get; set; }

        /// <summary>
        /// Any meta field not mapped above (e.g. "duration"), keyed by its JSON name.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
