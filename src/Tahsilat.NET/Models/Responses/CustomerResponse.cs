using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;

namespace Tahsilat.NET.Models.Responses
{
    public class CustomerResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("merchant_id")]
        public long MerchantId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

        [JsonProperty("name_lastname")]
        public string NameLastName { get; set; }

        [JsonProperty("phone_code")]
        public string PhoneCode { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("country_flag_url")]
        public string CountryFlagUrl { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }

        [JsonProperty("created_via_payment")]
        public bool CreatedViaPayment { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("formatted_created_at")]
        public string FormattedCreatedAt { get; set; }

        [JsonProperty("metadata")]
        public List<MetadataEntry> Metadata { get; set; }

        [JsonProperty("timeline")]
        public List<TimelineEntry> Timeline { get; set; }
    }
}
