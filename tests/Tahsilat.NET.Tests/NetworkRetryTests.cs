using System.Net;
using System.Net.Sockets;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.Tests
{
    public class NetworkRetryTests
    {
        private const string Ok = "{\"status\":true,\"data\":{\"id\":1}}";

        private static HttpRequestException ConnectFailure() =>
            new HttpRequestException("No connection could be made", new SocketException((int)SocketError.ConnectionRefused));

        private static HttpRequestException MidStreamFailure() =>
            new HttpRequestException("The response ended prematurely", new IOException("connection reset"));

        [Fact]
        public async Task ConnectFailure_IsRetriedForPost()
        {
            var (client, http) = TestClient.WithToken();
            http.EnqueueException(ConnectFailure());
            http.EnqueueException(ConnectFailure());
            http.Enqueue(HttpStatusCode.OK, Ok);

            var product = await client.Products.CreateAsync(new ProductCreateRequest { ProductName = "x", Price = 100 });

            Assert.Equal(1, product.Id);
            Assert.Equal(3, http.Requests.Count(r => !r.IsTokenRequest));
            Assert.All(http.Requests.Where(r => !r.IsTokenRequest), r => Assert.Equal("x", r.Form["product_name"]));
        }

        [Fact]
        public async Task ConnectFailure_GivesUpAfterMaxRetries()
        {
            var (client, http) = TestClient.WithToken(maxNetworkRetries: 2);
            http.EnqueueException(ConnectFailure());
            http.EnqueueException(ConnectFailure());
            http.EnqueueException(ConnectFailure());

            var ex = await Assert.ThrowsAsync<TahsilatNetworkException>(() => client.Products.GetAsync(1));

            Assert.IsType<HttpRequestException>(ex.InnerException);
            Assert.Null(ex.ErrorCode);
            Assert.Equal(3, http.Requests.Count(r => !r.IsTokenRequest));
        }

        [Fact]
        public async Task ZeroRetries_SendsOnce()
        {
            var (client, http) = TestClient.WithToken(maxNetworkRetries: 0);
            http.EnqueueException(ConnectFailure());

            await Assert.ThrowsAsync<TahsilatNetworkException>(() => client.Products.GetAsync(1));

            Assert.Single(http.Requests, r => !r.IsTokenRequest);
        }

        [Fact]
        public async Task MidStreamFailure_IsRetriedForGet_ButNotForPost()
        {
            var (getClient, getHttp) = TestClient.WithToken();
            getHttp.EnqueueException(MidStreamFailure());
            getHttp.Enqueue(HttpStatusCode.OK, Ok);

            await getClient.Products.GetAsync(1);
            Assert.Equal(2, getHttp.Requests.Count(r => !r.IsTokenRequest));

            var (postClient, postHttp) = TestClient.WithToken();
            postHttp.EnqueueException(MidStreamFailure());

            await Assert.ThrowsAsync<TahsilatNetworkException>(() =>
                postClient.Products.CreateAsync(new ProductCreateRequest { ProductName = "x", Price = 100 }));
            Assert.Single(postHttp.Requests, r => !r.IsTokenRequest);
        }

        [Fact]
        public async Task Timeout_IsRetriedForGet()
        {
            var (client, http) = TestClient.WithToken();
            http.EnqueueException(new TaskCanceledException("timeout"));
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Products.GetAsync(1);

            Assert.Equal(2, http.Requests.Count(r => !r.IsTokenRequest));
        }

        [Fact]
        public async Task Timeout_IsNeverRetriedForPost_AndExplainsWhy()
        {
            // Real HttpClient timeout: the handler takes longer than TimeoutSeconds.
            var (client, http) = TestClient.WithToken(timeoutSeconds: 1);
            http.Enqueue(async (_, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                return FakeHttpMessageHandler.JsonResponse(HttpStatusCode.OK, Ok);
            });

            var ex = await Assert.ThrowsAsync<TahsilatNetworkException>(() =>
                client.Transactions.RefundAsync(new RefundCreateRequest { TransactionId = 1, Description = "x" }));

            Assert.Contains("timed out after 1 seconds", ex.Message);
            Assert.Contains("query the transaction", ex.Message);
            Assert.NotNull(ex.InnerException);
            Assert.Single(http.Requests, r => !r.IsTokenRequest);
        }

        [Fact]
        public async Task TokenRequestTimeout_IsRetried()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueException(new TaskCanceledException("timeout"));
            http.EnqueueToken("tok_1", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Products.GetAsync(1);

            Assert.Equal(2, http.Requests.Count(r => r.IsTokenRequest));
        }

        [Fact]
        public async Task CallerCancellation_IsNotTurnedIntoANetworkError()
        {
            var (client, http) = TestClient.WithToken();
            using var cts = new CancellationTokenSource();
            http.Enqueue(async (_, ct) =>
            {
                cts.Cancel();
                await Task.Delay(Timeout.Infinite, ct);
                return FakeHttpMessageHandler.JsonResponse(HttpStatusCode.OK, Ok);
            });

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.Products.GetAsync(1, cts.Token));

            Assert.Single(http.Requests, r => !r.IsTokenRequest);
        }
    }
}
