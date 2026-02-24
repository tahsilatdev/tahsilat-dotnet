using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Requests
{
    public class CustomerCreateRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }
        
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }

        [JsonProperty("metadata")]
        public List<Dictionary<string, object>> Metadata { get; set; }
    }
}
