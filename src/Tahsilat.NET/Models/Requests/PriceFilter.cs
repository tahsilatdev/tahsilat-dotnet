using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Requests
{
    /// <summary>
    /// Price comparison for the product search: <c>financial[price]</c> compared
    /// with <c>financial[operator]</c>.
    /// </summary>
    public class PriceFilter
    {
        /// <summary>
        /// Price in minor units (kuruş), like everywhere else in the API.
        /// </summary>
        [JsonProperty("price")]
        public int? Price { get; set; }

        /// <summary>
        /// Comparison operator the API applies, e.g. "=", "&gt;", "&gt;=", "&lt;", "&lt;=".
        /// </summary>
        [JsonProperty("operator")]
        public string Operator { get; set; }
    }
}
