using System.Net;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.Tests
{
    public class ErrorMappingTests
    {
        [Fact]
        public async Task Success_WithStatusFalse_ThrowsWithTheApiErrorCode()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK,
                "{\"status\":false,\"message\":\"Banka iadeyi reddetti\",\"error_code\":\"5001\",\"data\":null}");

            var ex = await Assert.ThrowsAsync<TahsilatApiException>(() =>
                client.Transactions.RefundAsync(new RefundCreateRequest { TransactionId = 1, Description = "x" }));

            Assert.Equal("5001", ex.ErrorCode);
            Assert.Equal(200, ex.StatusCode);
            Assert.Equal("Banka iadeyi reddetti", ex.Message);
        }

        [Fact]
        public async Task Conflict_ThrowsTahsilatConflictException()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.Conflict,
                "{\"status\":false,\"message\":\"Aktif aboneliği olan ürün silinemez\",\"error_code\":\"4010\"}");

            var ex = await Assert.ThrowsAsync<TahsilatConflictException>(() => client.Products.DeleteAsync(1));

            Assert.Equal("4010", ex.ErrorCode);
        }

        [Fact]
        public async Task UnprocessableEntity_ExposesFieldErrorsAsDictionary()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue((HttpStatusCode)422,
                "{\"status\":false,\"message\":\"Doğrulama hatası\",\"error_code\":\"901\"," +
                "\"errors\":{\"amount\":[\"Tutar zorunludur.\",\"Tutar en az 100 olmalı.\"],\"currency\":[\"Geçersiz para birimi.\"]}}");

            var ex = await Assert.ThrowsAsync<TahsilatValidationException>(() =>
                client.Payments.CreateAsync(new PaymentCreateRequest { Amount = 0 }));

            Assert.Equal("901", ex.ErrorCode);
            Assert.Equal(2, ex.Errors.Count);
            Assert.Equal(new[] { "Tutar zorunludur.", "Tutar en az 100 olmalı." }, ex.Errors["amount"]);
            Assert.Equal(new[] { "Geçersiz para birimi." }, ex.Errors["currency"]);
            Assert.Contains("amount: Tutar zorunludur.", ex.Message);
        }

        [Fact]
        public async Task ArrayFormErrors_LandUnderTheGeneralKey()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.BadRequest,
                "{\"status\":false,\"message\":\"Hatalı istek\",\"errors\":[\"Ürün bulunamadı\"]}");

            var ex = await Assert.ThrowsAsync<TahsilatValidationException>(() => client.Products.GetAsync(1));

            Assert.Equal(new[] { "Ürün bulunamadı" }, ex.Errors["*"]);
            Assert.Null(ex.ErrorCode);
        }

        [Fact]
        public async Task ValidationError_WithoutErrorsBlock_HasEmptyDictionary()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue((HttpStatusCode)422, "{\"status\":false,\"message\":\"Kayıt bulunamadı\",\"error_code\":\"901\"}");

            var ex = await Assert.ThrowsAsync<TahsilatValidationException>(() => client.Products.GetAsync(1));

            Assert.Empty(ex.Errors);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, typeof(TahsilatNotFoundException))]
        [InlineData((HttpStatusCode)402, typeof(TahsilatPaymentException))]
        [InlineData((HttpStatusCode)424, typeof(TahsilatNetworkException))]
        [InlineData(HttpStatusCode.InternalServerError, typeof(TahsilatApiException))]
        public async Task HttpStatus_MapsToExceptionType(HttpStatusCode status, Type expected)
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(status, "{\"status\":false,\"message\":\"x\",\"error_code\":\"42\"}");

            var ex = await Assert.ThrowsAnyAsync<TahsilatException>(() => client.Products.GetAsync(1));

            Assert.IsType(expected, ex);
            Assert.Equal("42", ex.ErrorCode);
        }

        [Fact]
        public async Task RateLimit_ReadsRetryAfterHeader()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue((_, _) =>
            {
                var response = FakeHttpMessageHandler.JsonResponse((HttpStatusCode)429, "{\"status\":false,\"message\":\"slow down\"}");
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(7));
                return Task.FromResult(response);
            });

            var ex = await Assert.ThrowsAsync<TahsilatRateLimitException>(() => client.Products.GetAsync(1));

            Assert.Equal(7, ex.RetryAfterSeconds);
        }

        [Fact]
        public async Task NonJsonErrorBody_StillMapsByStatus()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                Content = new StringContent("<html>502</html>")
            }));

            var ex = await Assert.ThrowsAsync<TahsilatApiException>(() => client.Products.GetAsync(1));

            Assert.Equal(502, ex.StatusCode);
            Assert.Null(ex.ErrorCode);
        }
    }
}
