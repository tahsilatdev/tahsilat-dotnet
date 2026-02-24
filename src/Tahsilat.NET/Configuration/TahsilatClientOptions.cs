using System;
using Tahsilat.NET.Infrastructure.Logging;

namespace Tahsilat.NET.Configuration
{
    public class TahsilatClientOptions
    {
        private string _apiKey = string.Empty;

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                ValidateApiKey(value);
                _apiKey = value;
            }
        }

        public int TimeoutSeconds { get; set; } = 30;

        public ITahsilatLogger? Logger { get; set; }

        internal bool IsSandbox => _apiKey.StartsWith("sk_test_", StringComparison.OrdinalIgnoreCase);

        internal string BaseUrl => IsSandbox
            ? TahsilatConstants.SandboxBaseUrl
            : TahsilatConstants.ProductionBaseUrl;

        private static void ValidateApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("Secret key must not be null or empty.", nameof(apiKey));

            if (!apiKey.StartsWith("sk_test_", StringComparison.OrdinalIgnoreCase) &&
                !apiKey.StartsWith("sk_live_", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Invalid secret key format. Key must start with 'sk_test_' (sandbox) or 'sk_live_' (production).",
                    nameof(apiKey));
            }
        }
    }
}
