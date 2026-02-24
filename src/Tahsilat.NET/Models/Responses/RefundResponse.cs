using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Models.Responses
{
    public class RefundResponse
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("errors")]
        public object Errors { get; set; }

        [JsonProperty("error_code")]
        public string Error_code { get; set; }

        [JsonProperty("data")]
        public object? Data { get; set; }
    }
}
