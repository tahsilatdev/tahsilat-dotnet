using System;
using System.Net.Http;

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

        /// <summary>
        /// Time allowed for one HTTP exchange. 80 seconds by default (as in
        /// tahsilat-php): a payment or refund waits on the bank, and cutting it off
        /// early leaves you unsure whether it was processed.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 80;

        /// <summary>
        /// How many times a request is resent after a transport failure, 3 by
        /// default; 0 disables retries. A failure to connect (DNS, TCP, TLS) is
        /// retried for every request because the server never saw it. A timeout
        /// or a dropped connection is retried only for GET requests and the token
        /// request: a POST that timed out may still have been processed, and
        /// sending it again could create a second payment or refund.
        /// </summary>
        public int MaxNetworkRetries { get; set; } = 3;

        /// <summary>
        /// Optional handler the SDK sends requests through: a proxy, extra logging
        /// or a test double. The SDK does not dispose it. When null the SDK creates
        /// its own handler.
        /// </summary>
        public HttpMessageHandler? HttpMessageHandler { get; set; }

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
