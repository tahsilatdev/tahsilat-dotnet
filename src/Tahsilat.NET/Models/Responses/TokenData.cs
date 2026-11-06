using Newtonsoft.Json;
using System;

namespace Tahsilat.NET.Models.Responses
{
    internal sealed class TokenData
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = default!;

        /// <summary>
        /// Absolute expiry instant. The API sends an ISO 8601 value with an offset
        /// (e.g. "2026-11-06T06:55:17+03:00"), which DateTimeOffset preserves.
        /// </summary>
        [JsonProperty("expires_at")]
        public DateTimeOffset ExpiresAt { get; set; }

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
