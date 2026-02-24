using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tahsilat.NET.Models.Responses
{
    internal sealed class TokenData
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = default!;

        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [JsonProperty("supports_3d")]
        public bool Supports3d { get; set; }

        [JsonProperty("supports_2d")]
        public bool Supports2d { get; set; }

        [JsonProperty("supports_white_label")]
        public bool SupportsWhiteLabel { get; set; }

        [JsonProperty("supports_installment")]
        public bool SupportsInstallment { get; set; }
    }
}
