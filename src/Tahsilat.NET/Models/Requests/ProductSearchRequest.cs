using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Requests
{
    /// <summary>
    /// Filters for <c>GET products/search</c>. Every property is optional; null
    /// properties are not sent. Filters combine with AND.
    /// </summary>
    public class ProductSearchRequest
    {
        [JsonProperty("product_id")]
        public long? ProductId { get; set; }

        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        /// <summary>
        /// Price comparison. Sent as financial[price] / financial[operator].
        /// </summary>
        [JsonProperty("financial")]
        public PriceFilter Financial { get; set; }

        /// <summary>
        /// Creation date range. Sent as product_date[start_date] / product_date[end_date].
        /// </summary>
        [JsonProperty("product_date")]
        public DateRangeFilter ProductDate { get; set; }

        /// <summary>
        /// Match a metadata entry. Sent as metadata[key] / metadata[value].
        /// </summary>
        [JsonProperty("metadata")]
        public MetadataFilter Metadata { get; set; }

        /// <summary>
        /// 1-based page number. Use either <see cref="Page"/> or <see cref="Start"/>.
        /// </summary>
        [JsonProperty("page")]
        public int? Page { get; set; }

        /// <summary>
        /// Zero-based offset of the first row. Use either <see cref="Page"/> or <see cref="Start"/>.
        /// </summary>
        [JsonProperty("start")]
        public int? Start { get; set; }

        /// <summary>
        /// Rows per page, 1-100.
        /// </summary>
        [JsonProperty("limit")]
        public int? Limit { get; set; }
    }
}
