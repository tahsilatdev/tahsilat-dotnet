using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tahsilat.NET.Models.Requests
{
    public class TransactionQueryRequest
    {
        [JsonProperty("payment_id")]
        public long PaymentId { get; set; }
    }
}
