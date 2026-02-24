using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Common
{
    public class ApiResponse<T> where T : class
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
