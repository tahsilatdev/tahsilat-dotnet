using System.Net;
using Tahsilat.NET.Infrastructure.Http;
using Tahsilat.NET.Models.Enums;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.Tests
{
    /// <summary>
    /// What leaves the SDK on the wire: method, path, headers, encoded body.
    /// </summary>
    public class RequestShapeTests
    {
        private const string EmptyOk = "{\"status\":true,\"message\":\"ok\",\"data\":null}";

        [Fact]
        public async Task Delete_SendsJsonContentType()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK, EmptyOk);

            var deleted = await client.Products.DeleteAsync(42);

            var request = http.Requests.Single(r => !r.IsTokenRequest);
            Assert.True(deleted);
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.EndsWith("/products/42", request.Path);
            Assert.StartsWith("application/json", request.ContentType);
        }

        [Fact]
        public async Task EveryRequest_CarriesVersionedUserAgentAndBearerToken()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK, EmptyOk);

            await client.Products.DeleteAsync(1);

            var request = http.Requests.Single(r => !r.IsTokenRequest);
            Assert.Equal("tok_1", request.BearerToken);
            Assert.Matches(@"^tahsilat-dotnet/\d+\.\d+\.\d+$", request.UserAgent);
            Assert.Equal(HttpTransport.UserAgent, request.UserAgent);
        }

        [Fact]
        public async Task Metadata_TurkishCharactersAreSentVerbatim()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK, "{\"status\":true,\"data\":{\"id\":1}}");

            await client.Customers.CreateAsync(new CustomerCreateRequest
            {
                Name = "Ayşe",
                LastName = "Güçlü",
                Metadata = new()
                {
                    new Dictionary<string, object> { ["branch"] = "İstanbul şubesi-1" }
                }
            });

            var form = http.Requests.Single(r => !r.IsTokenRequest).Form;
            Assert.Equal("branch", form["metadata[0][key]"]);
            Assert.Equal("İstanbul şubesi-1", form["metadata[0][value]"]);
            Assert.Equal("Ayşe", form["name"]);
        }

        [Fact]
        public async Task SubscriptionFields_AreSentAsNumericCodes()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK, "{\"status\":true,\"data\":{\"id\":1}}");

            await client.Products.CreateAsync(new ProductCreateRequest
            {
                ProductName = "Premium Üyelik",
                Price = 49900,
                IsRecurring = true,
                BillingInterval = BillingInterval.Month,
                IntervalCount = 1
            });

            var form = http.Requests.Single(r => !r.IsTokenRequest).Form;
            Assert.Equal("1", form["is_recurring"]);
            Assert.Equal("3", form["billing_interval"]);
            Assert.Equal("1", form["interval_count"]);
        }

        [Fact]
        public async Task Payment_SendsSubscriptionCredentialAndWhiteLabelFields()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK, "{\"status\":true,\"data\":{\"transaction_id\":1,\"payment_page_url\":\"https://p\"}}");

            await client.Payments.CreateAsync(new PaymentCreateRequest
            {
                Amount = 49900,
                Currency = "TRY",
                ProductIds = new List<long> { 7, 8 },
                SubscriptionCredentialId = 123,
                WhiteLabel = true,
                InstallmentCount = 3
            });

            var form = http.Requests.Single(r => !r.IsTokenRequest).Form;
            Assert.Equal("7", form["product_ids[0]"]);
            Assert.Equal("8", form["product_ids[1]"]);
            Assert.Equal("123", form["subscription_credential_id"]);
            Assert.Equal("1", form["white_label"]);
            Assert.Equal("3", form["installment_count"]);
        }

        [Fact]
        public async Task UpdateMetadata_PostsTheWholeListToTheUpdateMetadataPath()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK, "{\"status\":true,\"data\":{\"id\":5,\"metadata\":[{\"key\":\"category\",\"value\":\"new\"}]}}");

            var product = await client.Products.UpdateMetadataAsync(5, new()
            {
                new Dictionary<string, object> { ["category"] = "new" },
                new Dictionary<string, object> { ["source"] = "sdk" }
            });

            var request = http.Requests.Single(r => !r.IsTokenRequest);
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.EndsWith("/products/update-metadata/5", request.Path);
            Assert.Equal("category", request.Form["metadata[0][key]"]);
            Assert.Equal("new", request.Form["metadata[0][value]"]);
            Assert.Equal("source", request.Form["metadata[1][key]"]);
            Assert.Equal("new", product.Metadata[0].Value);
        }

        [Fact]
        public async Task CommissionSearch_SendsBinInQueryStringWithoutBody()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK, "{\"status\":true,\"data\":[]}");

            await client.Commissions.SearchAsync(new CommissionSearchRequest { BinNumber = 489455 });

            var request = http.Requests.Single(r => !r.IsTokenRequest);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.EndsWith("/pos/commissions", request.Path);
            Assert.Equal("489455", request.QueryPairs["bin_number"]);
            Assert.Null(request.Body);
        }
    }
}
