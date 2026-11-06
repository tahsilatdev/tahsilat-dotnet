using System.Net;
using Tahsilat.NET.Exceptions;
using Xunit;

namespace Tahsilat.NET.Tests
{
    public class TokenLifecycleTests
    {
        private const string Ok = "{\"status\":true,\"data\":[]}";
        private const string InvalidToken403 = "{\"status\":false,\"message\":\"Geçersiz token\",\"error_code\":\"1002\"}";

        [Fact]
        public void Constructor_MakesNoRequests()
        {
            var (_, http) = TestClient.Create();
            Assert.Empty(http.Requests);
        }

        [Fact]
        public async Task FirstCall_MintsTokenWithSecretKey_ThenUsesItAsBearer()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_1", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Commissions.SearchAsync();

            Assert.Equal(2, http.Requests.Count);
            Assert.True(http.Requests[0].IsTokenRequest);
            Assert.Equal(HttpMethod.Post, http.Requests[0].Method);
            Assert.Equal(TestClient.ApiKey, http.Requests[0].BearerToken);
            Assert.StartsWith("application/json", http.Requests[0].ContentType);
            Assert.Equal("tok_1", http.Requests[1].BearerToken);
        }

        [Fact]
        public async Task SecondCall_ReusesUnexpiredToken()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_1", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.OK, Ok);
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Commissions.SearchAsync();
            await client.Commissions.SearchAsync();

            Assert.Single(http.Requests, r => r.IsTokenRequest);
        }

        [Fact]
        public async Task ExpiredToken_WithPlusThreeOffset_IsRefreshedBeforeUse()
        {
            // expires_at is written with the API's +03:00 offset and lies 30 minutes
            // in the past. The old DateTime comparison ignored the offset's Kind
            // and would have kept using this token for hours on a UTC+3 server.
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_stale", TimeSpan.FromMinutes(-30));
            http.Enqueue(HttpStatusCode.OK, Ok);
            http.EnqueueToken("tok_fresh", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Commissions.SearchAsync();
            await client.Commissions.SearchAsync();

            Assert.Equal(2, http.Requests.Count(r => r.IsTokenRequest));
            Assert.Equal("tok_fresh", http.Requests[3].BearerToken);
        }

        [Fact]
        public async Task TokenInsideSafetyMargin_IsRefreshedBeforeUse()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_short", TimeSpan.FromSeconds(30)); // inside the 60s margin
            http.Enqueue(HttpStatusCode.OK, Ok);
            http.EnqueueToken("tok_fresh", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Commissions.SearchAsync();
            await client.Commissions.SearchAsync();

            Assert.Equal("tok_fresh", http.Requests[3].BearerToken);
        }

        [Fact]
        public async Task TokenWellWithinLifetime_IsNotRefreshed()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_1", TimeSpan.FromSeconds(120)); // outside the 60s margin
            http.Enqueue(HttpStatusCode.OK, Ok);
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Commissions.SearchAsync();
            await client.Commissions.SearchAsync();

            Assert.Single(http.Requests, r => r.IsTokenRequest);
        }

        [Fact]
        public async Task Forbidden1002_MintsFreshTokenAndRetriesOnce()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_stale", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.Forbidden, InvalidToken403);
            http.EnqueueToken("tok_new", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Commissions.SearchAsync();

            Assert.Equal(4, http.Requests.Count);
            Assert.Equal("tok_stale", http.Requests[1].BearerToken);
            Assert.True(http.Requests[2].IsTokenRequest);
            Assert.Equal("tok_new", http.Requests[3].BearerToken);
        }

        [Fact]
        public async Task Unauthorized_IsAlsoRetriedOnce()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_1", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.Unauthorized, "{\"status\":false,\"message\":\"Unauthenticated\"}");
            http.EnqueueToken("tok_2", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.OK, Ok);

            await client.Commissions.SearchAsync();

            Assert.Equal("tok_2", http.Requests[3].BearerToken);
        }

        [Fact]
        public async Task SecondRejection_Propagates()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_1", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.Forbidden, InvalidToken403);
            http.EnqueueToken("tok_2", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.Forbidden, InvalidToken403);

            var ex = await Assert.ThrowsAsync<TahsilatAuthenticationException>(() => client.Commissions.SearchAsync());

            Assert.Equal("1002", ex.ErrorCode);
            Assert.Equal(403, ex.StatusCode);
            Assert.Equal(4, http.Requests.Count);
        }

        [Fact]
        public async Task Forbidden_WithoutCode1002_IsNotRetried()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_1", TimeSpan.FromMinutes(10));
            http.Enqueue(HttpStatusCode.Forbidden, "{\"status\":false,\"message\":\"Bu işlem için yetkiniz yok\",\"error_code\":\"1003\"}");

            var ex = await Assert.ThrowsAsync<TahsilatAuthenticationException>(() => client.Commissions.SearchAsync());

            Assert.Equal("1003", ex.ErrorCode);
            Assert.Equal(2, http.Requests.Count);
        }

        [Fact]
        public async Task ValidationError_IsNotRetried()
        {
            var (client, http) = TestClient.Create();
            http.EnqueueToken("tok_1", TimeSpan.FromMinutes(10));
            http.Enqueue((HttpStatusCode)422, "{\"status\":false,\"message\":\"validation\",\"error_code\":\"901\"}");

            await Assert.ThrowsAsync<TahsilatValidationException>(() => client.Commissions.SearchAsync());

            Assert.Equal(2, http.Requests.Count);
        }

        [Fact]
        public async Task TokenRequestFailure_IsNotRetriedWithANewToken()
        {
            var (client, http) = TestClient.Create();
            http.Enqueue(HttpStatusCode.Forbidden, "{\"status\":false,\"message\":\"Geçersiz anahtar\",\"error_code\":\"1001\"}");

            var ex = await Assert.ThrowsAsync<TahsilatAuthenticationException>(() => client.Commissions.SearchAsync());

            Assert.Equal("1001", ex.ErrorCode);
            Assert.Single(http.Requests);
        }

        [Fact]
        public async Task ConcurrentFirstCalls_MintOneToken()
        {
            var (client, http) = TestClient.Create();
            http.Enqueue(async (_, _) =>
            {
                await Task.Delay(100);
                return FakeHttpMessageHandler.JsonResponse(HttpStatusCode.OK, FakeHttpMessageHandler.TokenJson("tok_1", TimeSpan.FromMinutes(10)));
            });
            for (int i = 0; i < 5; i++) http.Enqueue(HttpStatusCode.OK, Ok);

            await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => client.Commissions.SearchAsync()));

            Assert.Single(http.Requests, r => r.IsTokenRequest);
            Assert.All(http.Requests.Where(r => !r.IsTokenRequest), r => Assert.Equal("tok_1", r.BearerToken));
        }
    }
}
