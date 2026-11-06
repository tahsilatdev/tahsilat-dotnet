using System.Net;
using System.Text;

namespace Tahsilat.NET.Tests
{
    /// <summary>
    /// Scripted HttpMessageHandler: responses are dequeued in order and every
    /// request the SDK sends is recorded with its body already read.
    /// </summary>
    public sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> _responders = new();

        public List<RecordedRequest> Requests { get; } = new();

        public void Enqueue(HttpStatusCode status, string json)
        {
            Enqueue((_, _) => Task.FromResult(JsonResponse(status, json)));
        }

        public void Enqueue(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        {
            _responders.Enqueue(responder);
        }

        public void EnqueueException(Exception exception)
        {
            Enqueue((_, _) => throw exception);
        }

        /// <summary>
        /// A successful token answer with expires_at expressed in the API's +03:00
        /// offset, exactly as the real server sends it.
        /// </summary>
        public void EnqueueToken(string token, TimeSpan timeToLive)
        {
            Enqueue(HttpStatusCode.OK, TokenJson(token, timeToLive));
        }

        public static string TokenJson(string token, TimeSpan timeToLive)
        {
            var expiresAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).Add(timeToLive);
            return "{\"status\":true,\"message\":\"ok\",\"data\":{\"access_token\":\"" + token +
                   "\",\"expires_at\":\"" + expiresAt.ToString("yyyy-MM-dd'T'HH:mm:sszzz") + "\"}}";
        }

        public static HttpResponseMessage JsonResponse(HttpStatusCode status, string json)
        {
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        private readonly object _gate = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder;

            // Tests fire concurrent requests; Queue<T> and List<T> are not thread-safe.
            lock (_gate)
            {
                Requests.Add(new RecordedRequest(
                    request.Method,
                    request.RequestUri!,
                    request.Headers.Authorization?.Parameter,
                    request.Content?.Headers.ContentType?.ToString(),
                    body,
                    request.Headers.UserAgent.ToString()));

                if (_responders.Count == 0)
                    throw new InvalidOperationException($"No response queued for {request.Method} {request.RequestUri}");

                responder = _responders.Dequeue();
            }

            return await responder(request, cancellationToken);
        }
    }

    public sealed record RecordedRequest(
        HttpMethod Method,
        Uri Uri,
        string? BearerToken,
        string? ContentType,
        string? Body,
        string UserAgent)
    {
        public string Path => Uri.AbsolutePath;
        public string Query => Uri.Query;
        public bool IsTokenRequest => Path.EndsWith("/token/get-token", StringComparison.Ordinal);

        /// <summary>Form body split into decoded key/value pairs.</summary>
        public Dictionary<string, string> Form =>
            (Body ?? string.Empty)
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2))
                .ToDictionary(p => Uri.UnescapeDataString(p[0].Replace('+', ' ')), p => p.Length > 1 ? Uri.UnescapeDataString(p[1].Replace('+', ' ')) : string.Empty);

        /// <summary>Query string split into decoded key/value pairs.</summary>
        public Dictionary<string, string> QueryPairs =>
            Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2))
                .ToDictionary(p => Uri.UnescapeDataString(p[0]), p => p.Length > 1 ? Uri.UnescapeDataString(p[1]) : string.Empty);
    }
}
