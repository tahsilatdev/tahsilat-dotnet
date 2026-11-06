using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Tahsilat.NET.Abstractions;
using Tahsilat.NET.Configuration;

namespace Tahsilat.NET.IntegrationTests
{
    /// <summary>
    /// Shares ONE TahsilatClient across every test. xUnit creates a new test
    /// class instance per test method; a client per test would mint a token per
    /// test and trip the token endpoint's per-minute rate limit.
    ///
    /// Settings come from appsettings.json, overridden by appsettings.local.json
    /// (git-ignored): Tahsilat:ApiKey, Tahsilat:WebhookSecret and the optional
    /// transaction IDs used by the refund / pre-auth / retrieve tests.
    /// </summary>
    public abstract class TestBase
    {
        private static readonly Lazy<IConfiguration> Configuration = new(() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build());

        private static readonly Lazy<ITahsilatClient> SharedClient = new(() =>
            new TahsilatClient(new TahsilatClientOptions
            {
                ApiKey = Configuration.Value["Tahsilat:ApiKey"]
                    ?? throw new InvalidOperationException("Tahsilat:ApiKey is missing; put your sandbox key in appsettings.local.json."),
                TimeoutSeconds = 500
            }));

        protected static ITahsilatClient Client => SharedClient.Value;

        protected static string WebhookSecret => Configuration.Value["Tahsilat:WebhookSecret"] ?? string.Empty;

        /// <summary>
        /// A completed, refundable transaction in the sandbox (Tahsilat:RefundableTransactionId).
        /// 0 when not configured; the dependent test then returns without asserting.
        /// </summary>
        protected static long RefundableTransactionId => ReadLong("Tahsilat:RefundableTransactionId");

        /// <summary>
        /// An open pre-authorisation in the sandbox (Tahsilat:PreAuthTransactionId).
        /// </summary>
        protected static long PreAuthTransactionId => ReadLong("Tahsilat:PreAuthTransactionId");

        /// <summary>
        /// Any existing transaction in the sandbox (Tahsilat:TransactionId).
        /// </summary>
        protected static long KnownTransactionId => ReadLong("Tahsilat:TransactionId");

        private static long ReadLong(string key)
        {
            return long.TryParse(Configuration.Value[key], out var value) ? value : 0;
        }

        /// <summary>
        /// The search index lags a moment behind a create. Polls <paramref name="condition"/>
        /// about once a second for up to <paramref name="attempts"/> tries and returns
        /// the last result, so a search-after-create assertion does not race the API.
        /// </summary>
        protected static async System.Threading.Tasks.Task<bool> EventuallyAsync(
            System.Func<System.Threading.Tasks.Task<bool>> condition, int attempts = 6)
        {
            for (int i = 0; i < attempts; i++)
            {
                if (await condition())
                    return true;

                await System.Threading.Tasks.Task.Delay(1000);
            }

            return false;
        }
    }
}
