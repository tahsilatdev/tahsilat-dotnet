using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Configuration;
using Tahsilat.NET.Exceptions;

namespace Tahsilat.NET.Infrastructure.Http
{
    /// <summary>
    /// Owns the HttpClient and turns transport failures into
    /// <see cref="TahsilatNetworkException"/>, retrying where that is safe:
    /// a connection that was never established can be retried for any method,
    /// while a timeout or a mid-stream failure may already have been processed
    /// by the server and is only retried for requests the caller marks as safe
    /// to repeat (GET and the token request). A repeated POST could produce a
    /// second payment or refund.
    /// </summary>
    internal sealed class HttpTransport : IDisposable
    {
        /// <summary>User-Agent sent with every request, e.g. "tahsilat-dotnet/1.2.0".</summary>
        internal static readonly string UserAgent =
            "tahsilat-dotnet/" + (typeof(HttpTransport).Assembly.GetName().Version?.ToString(3) ?? "0.0.0");

        private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(2);
        private static readonly Random Jitter = new Random();

        private readonly HttpClient _client;
        private readonly int _maxNetworkRetries;
        private readonly TimeSpan _timeout;

        public HttpTransport(TahsilatClientOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.TimeoutSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(options.TimeoutSeconds), "TimeoutSeconds must be positive.");
            if (options.MaxNetworkRetries < 0) throw new ArgumentOutOfRangeException(nameof(options.MaxNetworkRetries), "MaxNetworkRetries cannot be negative.");

#if NET452
            // .NET 4.5.2 negotiates TLS 1.0 by default and the API requires TLS 1.2.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
#endif

            // A caller-supplied handler (proxy, test double) stays owned by the caller.
            _client = options.HttpMessageHandler != null
                ? new HttpClient(options.HttpMessageHandler, disposeHandler: false)
                : new HttpClient(CreateDefaultHandler(), disposeHandler: true);

            _client.BaseAddress = new Uri(options.BaseUrl);
            _client.Timeout = _timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            if (_client.BaseAddress.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Tahsilat API must be called over HTTPS.");

            _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
            _client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("tr");
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            _maxNetworkRetries = options.MaxNetworkRetries;
        }

        private static HttpMessageHandler CreateDefaultHandler()
        {
#if NET6_0_OR_GREATER
            // A long-lived HttpClient never notices DNS changes unless connections
            // are recycled.
            return new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) };
#else
            return new HttpClientHandler();
#endif
        }

        /// <summary>
        /// Sends <paramref name="template"/>, cloning it for every attempt so the
        /// caller can send the same message again (e.g. after a token refresh).
        /// The caller keeps ownership of the template.
        /// </summary>
        /// <param name="safeToRepeat">
        /// True when a duplicate delivery has no side effect (GET, token request).
        /// Only then are timeouts and mid-stream failures retried.
        /// </param>
        public async Task<RawResponse> SendAsync(HttpRequestMessage template, bool safeToRepeat, CancellationToken cancellationToken)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            for (int attempt = 0; ; attempt++)
            {
                var canRetry = attempt < _maxNetworkRetries;

                try
                {
                    using (var request = await CloneAsync(template).ConfigureAwait(false))
                    using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                    {
                        var body = response.Content == null
                            ? null
                            : await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        return new RawResponse(response.StatusCode, body, response.Headers.RetryAfter?.Delta);
                    }
                }
                catch (HttpRequestException ex) when (canRetry && (safeToRepeat || IsConnectionFailure(ex)))
                {
                    // Connection never established: the server saw nothing, any method may retry.
                    _ = ex;
                }
                catch (HttpRequestException ex)
                {
                    throw new TahsilatNetworkException("Could not reach the Tahsilat API: " + ex.Message, ex);
                }
                catch (IOException) when (canRetry && safeToRepeat)
                {
                    // Connection dropped while reading the response.
                }
                catch (IOException ex)
                {
                    throw new TahsilatNetworkException("Connection to the Tahsilat API was lost: " + ex.Message, ex);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && canRetry && safeToRepeat)
                {
                    // HttpClient timeout (not the caller's token). The server may still
                    // process the request, so only repeat-safe requests get here.
                }
                catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TahsilatNetworkException(
                        $"The request to the Tahsilat API timed out after {_timeout.TotalSeconds:0} seconds. " +
                        "A mutating request (payment, refund, pre-auth) may still have been processed; query the transaction before retrying.",
                        ex);
                }

                await Task.Delay(Backoff(attempt), cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 0.5s, 1s, 1.5s ... capped at 2s, plus up to 100ms of jitter so clients
        /// that failed together do not retry together.
        /// </summary>
        private static TimeSpan Backoff(int attempt)
        {
            int jitterMs;
            lock (Jitter) jitterMs = Jitter.Next(0, 100);

            var delay = TimeSpan.FromMilliseconds(500 * (attempt + 1) + jitterMs);
            return delay < MaxBackoff ? delay : MaxBackoff;
        }

        /// <summary>
        /// True when the failure happened before the request left the machine:
        /// DNS, TCP connect or TLS handshake. Only the direct inner exception is
        /// inspected because a connection reset in the middle of a response also
        /// carries a SocketException further down, and that case is not safe.
        /// </summary>
        private static bool IsConnectionFailure(HttpRequestException ex)
        {
            switch (ex.InnerException)
            {
                case SocketException _:
                case AuthenticationException _:
                    return true;

                case WebException web:
                    return web.Status == WebExceptionStatus.ConnectFailure
                        || web.Status == WebExceptionStatus.NameResolutionFailure
                        || web.Status == WebExceptionStatus.ProxyNameResolutionFailure
                        || web.Status == WebExceptionStatus.SecureChannelFailure
                        || web.Status == WebExceptionStatus.TrustFailure;

                default:
                    return false;
            }
        }

        /// <summary>
        /// An HttpRequestMessage can be sent only once, and .NET Framework disposes
        /// its content after sending, so every attempt gets its own copy. Request
        /// bodies are small form payloads already in memory; buffering is cheap.
        /// </summary>
        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            if (request.Content != null)
            {
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

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    /// <summary>
    /// Status and body of one HTTP exchange, read fully so the caller can parse
    /// it without holding the connection.
    /// </summary>
    internal sealed class RawResponse
    {
        public HttpStatusCode StatusCode { get; }
        public string? Body { get; }
        public TimeSpan? RetryAfter { get; }

        public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode <= 299;

        public RawResponse(HttpStatusCode statusCode, string? body, TimeSpan? retryAfter)
        {
            StatusCode = statusCode;
            Body = body;
            RetryAfter = retryAfter;
        }
    }
}
