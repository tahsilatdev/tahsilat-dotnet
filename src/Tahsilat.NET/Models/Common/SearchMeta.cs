using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// Pagination block (<c>data.meta</c>) returned by the search endpoints.
    /// Every property is nullable because the API only includes the fields that
    /// apply to the paging mode used (page vs. start). Fields the SDK does not
    /// model yet are kept in <see cref="Extra"/>.
    /// </summary>
    public class SearchMeta
    {
        /// <summary>Total number of rows matching the filters.</summary>
        [JsonProperty("total")]
        public int? Total { get; set; }

        /// <summary>Rows per page that was applied.</summary>
        [JsonProperty("limit")]
        public int? Limit { get; set; }

        /// <summary>Current 1-based page, when paging by page.</summary>
        [JsonProperty("page")]
        public int? Page { get; set; }

        /// <summary>Zero-based offset of the first row, when paging by start.</summary>
        [JsonProperty("start")]
        public int? Start { get; set; }

        /// <summary>Number of the last page, when the API reports it.</summary>
        [JsonProperty("last_page")]
        public int? LastPage { get; set; }

        /// <summary>
        /// Any meta field not mapped above, keyed by its JSON name.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}
