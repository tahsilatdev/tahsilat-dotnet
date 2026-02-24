using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Configuration;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Infrastructure.Http
{
    internal sealed class TokenManager : IDisposable
    {
        private readonly string _secretKey;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private string? _accessToken;
        private DateTime _expiresAt;

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

        public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(_accessToken) &&
                DateTime.UtcNow < _expiresAt.AddMinutes(-5))
            {
                return _accessToken;
            }

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) &&
                    DateTime.UtcNow < _expiresAt.AddMinutes(-5))
                {
                    return _accessToken;
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "token/get-token");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
                request.Content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

                using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        ApiResponse<TokenData> errorResponse = null;
                        try { errorResponse = JsonConvert.DeserializeObject<ApiResponse<TokenData>>(content); } catch { }

                        throw new TahsilatAuthenticationException(
                            errorResponse?.Message ?? $"Token retrieval failed. Status: {response.StatusCode}",
                            errorResponse?.ErrorCode,
                            (int)response.StatusCode);
                    }

                    var result = JsonConvert.DeserializeObject<ApiResponse<TokenData>>(content);

                    if (result == null || !result.Status || result.Data == null)
                    {
                        throw new TahsilatAuthenticationException(
                            result?.Message ?? "Token response is invalid.",
                            result?.ErrorCode,
                            0);
                    }

                    _accessToken = result.Data.AccessToken;
                    _expiresAt = result.Data.ExpiresAt;

                    return _accessToken;
                }
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

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }
}
