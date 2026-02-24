using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Configuration;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Models.Common;

namespace Tahsilat.NET.Infrastructure.Http
{

    internal sealed class TahsilatHttpClient : ITahsilatHttpClient, IDisposable
    {
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

            if (!request.Headers.Contains(TahsilatConstants.HeaderAuthorization))
            {
                if (!request.Headers.Contains("Authorization"))
                {
                    var token = await _tokenManager.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            using (var response = await _client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false))
            {
                string content = response.Content == null
                    ? null
                    : await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    ThrowForError(response.StatusCode, content, response);

                if (string.IsNullOrWhiteSpace(content))
                    return default(TResponse);

                return JsonConvert.DeserializeObject<TResponse>(content);
            }
        }

        /// <summary>
        /// Sync wrapper — eski .NET Framework ortamları için.
        /// </summary>
        public TResponse SendAndRead<TResponse>(HttpRequestMessage request)
        {
            return Task.Run(() => SendAndReadAsync<TResponse>(request, CancellationToken.None)).GetAwaiter().GetResult();
        }

        private static void ThrowForError(HttpStatusCode code, string body, HttpResponseMessage response)
        {
            ApiErrorResponse err = null;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try { err = JsonConvert.DeserializeObject<ApiErrorResponse>(body); }
                catch { }
            }

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