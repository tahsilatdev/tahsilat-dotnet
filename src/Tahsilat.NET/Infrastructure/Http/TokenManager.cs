using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Infrastructure.Http
{
    internal sealed class TokenManager : IDisposable
    {
        /// <summary>
        /// Access tokens live 10 minutes. A token is treated as expired this long
        /// before its real expiry so a request already in flight never reaches the
        /// server with a dead token.
        /// </summary>
        internal static readonly TimeSpan RefreshMargin = TimeSpan.FromSeconds(60);

        private readonly string _secretKey;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        // Token and expiry are swapped together so a reader outside the lock never
        // sees a new token paired with an old expiry.
        private volatile CachedToken? _current;

        /// <summary>
        /// Initializes with an externally shared HttpClient.
        /// HttpClient lifecycle is managed externally.
        /// </summary>
        public TokenManager(string secretKey, HttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentNullException(nameof(secretKey));

            _secretKey = secretKey;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Returns the cached token, fetching a new one when none is cached or the
        /// cached one is within <see cref="RefreshMargin"/> of expiry.
        /// </summary>
        public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            var cached = _current;
            if (cached != null && cached.IsUsable)
                return cached.Value;

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                cached = _current;
                if (cached != null && cached.IsUsable)
                    return cached.Value;

                return await FetchAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Called after the server rejected <paramref name="rejectedToken"/>. Fetches
        /// a replacement unless another request already replaced it, so a burst of
        /// concurrent 403s costs one token request rather than one per caller.
        /// </summary>
        public async Task<string> RefreshAsync(string rejectedToken, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var cached = _current;
                if (cached != null && cached.IsUsable && !string.Equals(cached.Value, rejectedToken, StringComparison.Ordinal))
                    return cached.Value;

                return await FetchAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Sync wrapper — for legacy .NET Framework environments.
        /// </summary>
        public string GetToken()
        {
            return Task.Run(() => GetTokenAsync(CancellationToken.None)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Requests a token with the secret key and caches it. Must be called while
        /// holding the semaphore.
        /// </summary>
        private async Task<string> FetchAsync(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "token/get-token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
            request.Content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

            using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    ApiResponse<TokenData>? errorResponse = null;
                    try { errorResponse = JsonConvert.DeserializeObject<ApiResponse<TokenData>>(content); } catch { }

                    throw new TahsilatAuthenticationException(
                        errorResponse?.Message ?? $"Token retrieval failed. Status: {response.StatusCode}",
                        errorResponse?.ErrorCode,
                        (int)response.StatusCode);
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<TokenData>>(content);

                if (result == null || !result.Status || result.Data == null || string.IsNullOrEmpty(result.Data.AccessToken))
                {
                    throw new TahsilatAuthenticationException(
                        result?.Message ?? "Token response is invalid.",
                        result?.ErrorCode,
                        0);
                }

                var fresh = new CachedToken(result.Data.AccessToken, result.Data.ExpiresAt);
                _current = fresh;
                return fresh.Value;
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }

        private sealed class CachedToken
        {
            public string Value { get; }

            // DateTimeOffset keeps the server's "+03:00" offset, so the comparison
            // below is between two absolute instants. The previous DateTime-based
            // check compared a Kind=Local value against UtcNow and treated an
            // expired token as valid for hours on a UTC+3 server.
            public DateTimeOffset ExpiresAt { get; }

            public CachedToken(string value, DateTimeOffset expiresAt)
            {
                Value = value;
                ExpiresAt = expiresAt;
            }

            public bool IsUsable => DateTimeOffset.UtcNow < ExpiresAt - RefreshMargin;
        }
    }
}
