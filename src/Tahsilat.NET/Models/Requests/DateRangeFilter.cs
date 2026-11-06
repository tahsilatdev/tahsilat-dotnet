using Newtonsoft.Json;
using System;

namespace Tahsilat.NET.Models.Requests
{
    /// <summary>
    /// Inclusive calendar date range used by the search endpoints. Only the date
    /// part is sent (yyyy-MM-dd); either bound may be left null.
    /// </summary>
    public class DateRangeFilter
    {
        [JsonProperty("start_date")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; }
    }
}
