using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Requests
{
    /// <summary>
    /// Filters for <c>GET customers/search</c>. Every property is optional; null
    /// properties are not sent. Filters combine with AND.
    /// </summary>
    public class CustomerSearchRequest
    {
        /// <summary>
        /// Free text matched against the customer's name, e-mail and phone.
        /// </summary>
        [JsonProperty("search")]
        public string Search { get; set; }

        [JsonProperty("customer_id")]
        public long? CustomerId { get; set; }

        /// <summary>
        /// Full name as stored by the API ("John Doe").
        /// </summary>
        [JsonProperty("name_lastname")]
        public string NameLastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Creation date range. Sent as customer_date[start_date] / customer_date[end_date].
        /// </summary>
        [JsonProperty("customer_date")]
        public DateRangeFilter CustomerDate { get; set; }

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
