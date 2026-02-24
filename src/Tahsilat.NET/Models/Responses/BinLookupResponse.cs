using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Enums;

namespace Tahsilat.NET.Models.Responses
{
    public class BinLookupResponse
    {
        [JsonProperty("bank_code")]
        public int BankCode { get; set; }

        [JsonProperty("bank_name")]
        public string BankName { get; set; }

        [JsonProperty("bank_image")]
        public string BankImage { get; set; }

        [JsonProperty("bin_number")]
        public long BinNumber { get; set; }

        [JsonProperty("card_brand")]
        public string CardBrand { get; set; }

        [JsonProperty("card_brand_image_url")]
        public string CardBrandImageUrl { get; set; }

        [JsonProperty("card_type")]
        public string CardType { get; set; }
    }
}
