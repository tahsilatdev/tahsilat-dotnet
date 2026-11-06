using Tahsilat.NET.Configuration;

namespace Tahsilat.NET.Tests
{
    internal static class TestClient
    {
        public const string ApiKey = "sk_test_unit";

        /// <summary>
        /// A client wired to a scripted handler. Tests that do not care about the
        /// token queue a token answer first with <see cref="WithToken"/>.
        /// </summary>
        public static (TahsilatClient Client, FakeHttpMessageHandler Http) Create(int maxNetworkRetries = 3, int timeoutSeconds = 80)
        {
            var http = new FakeHttpMessageHandler();
            var client = new TahsilatClient(new TahsilatClientOptions
            {
                ApiKey = ApiKey,
                HttpMessageHandler = http,
                MaxNetworkRetries = maxNetworkRetries,
                TimeoutSeconds = timeoutSeconds
            });
            return (client, http);
        }

        public static (TahsilatClient Client, FakeHttpMessageHandler Http) WithToken(int maxNetworkRetries = 3, int timeoutSeconds = 80)
        {
            var pair = Create(maxNetworkRetries, timeoutSeconds);
            pair.Http.EnqueueToken("tok_1", TimeSpan.FromMinutes(10));
            return pair;
        }
    }
}
