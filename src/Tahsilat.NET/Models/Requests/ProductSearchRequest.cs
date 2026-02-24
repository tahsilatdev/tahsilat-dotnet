using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tahsilat.NET.Models.Requests
{
    public class ProductSearchRequest
    {
        [JsonProperty("keyword")]
        public string Keyword { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("page_size")]
        public int PageSize { get; set; }
    }
}
