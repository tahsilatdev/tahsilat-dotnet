using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tahsilat.NET.Models.Common
{
    public class TimelineEntry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}
