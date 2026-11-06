using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Configuration;
using Tahsilat.NET.Exceptions;

namespace Tahsilat.NET.Infrastructure.Http
{
    /// <summary>
    /// Attaches the access token, maps API errors to exceptions and retries once
    /// with a fresh token when the server rejects the current one.
    /// </summary>
    internal sealed class TahsilatHttpClient : ITahsilatHttpClient, IDisposable
    {
        /// <summary>
        /// error_code the API sends with a 403 when the access token is invalid or
        /// expired. Other 403s are real authorisation failures and are not retried.
        /// </summary>
        internal const string ErrorCodeInvalidToken = "1002";

        private readonly HttpTransport _transport;
        private readonly TokenManager _tokenManager;

        public TahsilatHttpClient(TahsilatClientOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _transport = new HttpTransport(options);
            _tokenManager = new TokenManager(options.ApiKey, _transport);
        }

        public async Task<TResponse> SendAndReadAsync<TResponse>(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            using (request)
            {
                // Only GETs are safe to deliver twice; a repeated POST could create a
                // second payment or refund.
                var safeToRepeat = request.Method == HttpMethod.Get;

                // A caller that set its own Authorization header manages its own token;
                // only SDK-managed tokens are refreshed and retried below.
                if (request.Headers.Contains(TahsilatConstants.HeaderAuthorization))
                {
                    var own = await _transport.SendAsync(request, safeToRepeat, cancellationToken).ConfigureAwait(false);
                    return Read<TResponse>(own);
                }

                var token = await _tokenManager.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var raw = await _transport.SendAsync(request, safeToRepeat, cancellationToken).ConfigureAwait(false);

                if (!raw.IsSuccess && IsTokenRejected(raw))
                {
                    // The server rejected the token (revoked, or expired in a way the
                    // local clock did not predict). Mint a fresh one and retry exactly
                    // once; a second failure surfaces as-is.
                    var freshToken = await _tokenManager.RefreshAsync(token, cancellationToken).ConfigureAwait(false);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", freshToken);

                    raw = await _transport.SendAsync(request, safeToRepeat, cancellationToken).ConfigureAwait(false);
                }

                return Read<TResponse>(raw);
            }
        }

        /// <summary>
        /// Sync wrapper — for legacy .NET Framework environments.
        /// </summary>
        public TResponse SendAndRead<TResponse>(HttpRequestMessage request)
        {
            return Task.Run(() => SendAndReadAsync<TResponse>(request, CancellationToken.None)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Throws for an HTTP error, then for a 2xx whose envelope says
        /// <c>"status": false</c> (a bank rejection on refund or pre-auth, for
        /// example), and otherwise deserialises the body.
        /// </summary>
        private static TResponse Read<TResponse>(RawResponse raw)
        {
            var err = ApiErrors.Parse(raw.Body);

            if (!raw.IsSuccess)
                throw ApiErrors.ForStatus(raw, err);

            if (err != null && err.Status == false)
                throw new TahsilatApiException(ApiErrors.FormatMessage(err, raw.StatusCode), (int)raw.StatusCode, err.ErrorCode);

            if (string.IsNullOrWhiteSpace(raw.Body))
                return default!;

            return JsonConvert.DeserializeObject<TResponse>(raw.Body!)!;
        }

        /// <summary>
        /// The API answers an invalid or expired token with 403 + error_code 1002
        /// (not 401). 401 is handled too in case that ever changes.
        /// </summary>
        private static bool IsTokenRejected(RawResponse raw)
        {
            if (raw.StatusCode == HttpStatusCode.Unauthorized)
                return true;

            if (raw.StatusCode != HttpStatusCode.Forbidden)
                return false;

            return string.Equals(ApiErrors.Parse(raw.Body)?.ErrorCode, ErrorCodeInvalidToken, StringComparison.Ordinal);
        }

        public void Dispose()
        {
            _tokenManager.Dispose();
            _transport.Dispose();
        }
    }
}
