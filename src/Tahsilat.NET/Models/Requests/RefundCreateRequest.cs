using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Requests
{
    public class RefundCreateRequest
    {
        /// <summary>
        /// İade edilecek işlemin ID'si. Zorunludur.
        /// </summary>
        [JsonProperty("transaction_id")]
        public long TransactionId { get; set; }

        /// <summary>
        /// İade tutarı. Opsiyoneldir: null bırakılırsa işlem tutarının tamamı iade
        /// edilir; değer verilirse kuruş cinsindendir (min 100 = 1,00 TL), işlem
        /// tutarından küçükse kısmi iade yapılır.
        /// </summary>
        [JsonProperty("amount")]
        public int? Amount { get; set; }

        /// <summary>
        /// İade açıklaması. Zorunludur, en fazla 255 karakter.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
