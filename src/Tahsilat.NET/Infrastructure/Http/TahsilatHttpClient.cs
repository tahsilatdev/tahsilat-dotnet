using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Configuration;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Models.Common;

namespace Tahsilat.NET.Infrastructure.Http
{

    internal sealed class TahsilatHttpClient : ITahsilatHttpClient, IDisposable
    {
        /// <summary>
        /// error_code the API sends with a 403 when the access token is invalid or
        /// expired. Other 403s are real authorisation failures and are not retried.
        /// </summary>
        internal const string ErrorCodeInvalidToken = "1002";

        private readonly HttpClient _client;
        private readonly TokenManager _tokenManager;

        public TahsilatHttpClient(TahsilatClientOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _client = new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl),
                Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
            };

            if (_client.BaseAddress.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Tahsilat API must be called over HTTPS.");

            _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "tahsilat-dotnet/1.0");
            _client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("tr");
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            _tokenManager = new TokenManager(options.ApiKey, _client);
        }

        public async Task<TResponse> SendAndReadAsync<TResponse>(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // A caller that set its own Authorization header manages its own token;
            // only SDK-managed tokens are refreshed and retried below.
            if (request.Headers.Contains(TahsilatConstants.HeaderAuthorization))
            {
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    var body = await ReadBodyAsync(response).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        ThrowForError(response, body);
                    return Deserialize<TResponse>(body);
                }
            }

            var token = await _tokenManager.GetTokenAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // An HttpRequestMessage can be sent once, and HttpClient on .NET Framework
            // disposes its content after sending, so the retry copy is built now.
            var retry = await CloneAsync(request).ConfigureAwait(false);
            try
            {
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    var body = await ReadBodyAsync(response).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                        return Deserialize<TResponse>(body);

                    if (!IsTokenRejected(response.StatusCode, body))
                        ThrowForError(response, body);
                }

                // The server rejected the token (revoked, or expired in a way the local
                // clock did not predict). Mint a fresh one and retry exactly once; a
                // second failure surfaces as-is.
                var freshToken = await _tokenManager.RefreshAsync(token, cancellationToken).ConfigureAwait(false);
                retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", freshToken);

                using (var response = await _client.SendAsync(retry, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    var body = await ReadBodyAsync(response).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        ThrowForError(response, body);
                    return Deserialize<TResponse>(body);
                }
            }
            finally
            {
                retry.Dispose();
            }
        }

        /// <summary>
        /// Sync wrapper — for legacy .NET Framework environments.
        /// </summary>
        public TResponse SendAndRead<TResponse>(HttpRequestMessage request)
        {
            return Task.Run(() => SendAndReadAsync<TResponse>(request, CancellationToken.None)).GetAwaiter().GetResult();
        }

        private static async Task<string?> ReadBodyAsync(HttpResponseMessage response)
        {
            return response.Content == null
                ? null
                : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private static TResponse Deserialize<TResponse>(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return default!;

            return JsonConvert.DeserializeObject<TResponse>(body!)!;
        }

        /// <summary>
        /// The API answers an invalid or expired token with 403 + error_code 1002
        /// (not 401). 401 is handled too in case that ever changes.
        /// </summary>
        private static bool IsTokenRejected(HttpStatusCode code, string? body)
        {
            if (code == HttpStatusCode.Unauthorized)
                return true;

            if (code != HttpStatusCode.Forbidden)
                return false;

            return string.Equals(ParseError(body)?.ErrorCode, ErrorCodeInvalidToken, StringComparison.Ordinal);
        }

        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            if (request.Content != null)
            {
                // Request bodies are small form/JSON payloads already held in memory,
                // so buffering them is cheap.
                var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                var content = new ByteArrayContent(bytes);
                foreach (var header in request.Content.Headers)
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                clone.Content = content;
            }

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;
        }

        private static ApiErrorResponse? ParseError(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;

            try { return JsonConvert.DeserializeObject<ApiErrorResponse>(body!); }
            catch { return null; }
        }

        private static void ThrowForError(HttpResponseMessage response, string? body)
        {
            var code = response.StatusCode;
            var err = ParseError(body);

            var errCode = err?.ErrorCode;
            var message = err?.Message ?? $"Tahsilat API error ({(int)code})";

            var errorList = err?.GetErrorList();
            if (errorList != null && errorList.Count > 0)
            {
                message = $"{message} [{string.Join("; ", errorList)}]";
            }

            switch (code)
            {
                case HttpStatusCode.BadRequest:
                case (HttpStatusCode)422:
                    throw new TahsilatValidationException(message, errCode);

                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    throw new TahsilatAuthenticationException(message, errCode, (int)code);

                case (HttpStatusCode)402:
                    throw new TahsilatPaymentException(message, errCode);

                case HttpStatusCode.NotFound:
                    throw new TahsilatNotFoundException(message, errCode);

                case (HttpStatusCode)429:
                    var retry = (int)(response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 0);
                    throw new TahsilatRateLimitException(message, retry, errCode);

                case (HttpStatusCode)424:
                    throw new TahsilatNetworkException(message, errCode);

                default:
                    throw new TahsilatApiException(message, (int)code, errCode);
            }
        }

        public void Dispose()
        {
            _tokenManager?.Dispose();
            _client?.Dispose();
        }
    }
}
