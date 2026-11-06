using System.Net;
using Tahsilat.NET.Infrastructure.Http;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.Tests
{
    public class SearchTests
    {
        [Fact]
        public async Task CustomerSearch_SendsFiltersEncodedInTheQueryString()
        {
            var (client, http) = TestClient.WithToken();
            // meta shape as the sandbox returns it, plus an unknown field.
            http.Enqueue(HttpStatusCode.OK,
                "{\"status\":true,\"data\":{\"meta\":{\"total\":1,\"limit\":10,\"page\":1,\"total_page\":1,\"sort\":null,\"duration\":null,\"next_cursor\":\"abc\"}," +
                "\"customers\":[{\"id\":9,\"name\":\"Ayşe\",\"email\":\"ayse@mail.com\",\"payment_link\":{\"has_payment_link\":false}}]}}");

            var page = await client.Customers.SearchAsync(new CustomerSearchRequest
            {
                Search = "ayşe & co",
                Email = "ayse@mail.com",
                CustomerDate = new DateRangeFilter
                {
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 1, 31)
                },
                Metadata = new MetadataFilter { Key = "source", Value = "sdk" },
                Page = 1,
                Limit = 10
            });

            var request = http.Requests.Single(r => !r.IsTokenRequest);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.EndsWith("/customers/search", request.Path);
            Assert.Null(request.Body);

            // Encoded on the wire, readable after decoding.
            Assert.Contains("search=ay%C5%9Fe%20%26%20co", request.Query);
            var q = request.QueryPairs;
            Assert.Equal("ayşe & co", q["search"]);
            Assert.Equal("ayse@mail.com", q["email"]);
            Assert.Equal("2026-01-01", q["customer_date[start_date]"]);
            Assert.Equal("2026-01-31", q["customer_date[end_date]"]);
            Assert.Equal("source", q["metadata[key]"]);
            Assert.Equal("sdk", q["metadata[value]"]);
            Assert.Equal("1", q["page"]);
            Assert.Equal("10", q["limit"]);
            Assert.DoesNotContain("customer_id", q.Keys);

            // { meta, customers } is read as a page, not as a list.
            Assert.Equal(1, page.Meta.Total);
            Assert.Equal(1, page.Meta.Page);
            Assert.Equal(10, page.Meta.Limit);
            Assert.Equal(1, page.Meta.TotalPage);
            Assert.Null(page.Meta.Sort);
            Assert.Equal("abc", page.Meta.Extra["next_cursor"]!.ToString());
            Assert.Single(page.Customers);
            Assert.Equal(9, page.Customers[0].Id);
            Assert.False(page.Customers[0].PaymentLink.HasPaymentLink);
        }

        [Fact]
        public async Task ProductSearch_SendsFinancialFilterAndReadsProductsPage()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK,
                "{\"status\":true,\"data\":{\"meta\":{\"total\":2,\"start\":0,\"limit\":50}," +
                "\"products\":[{\"id\":1,\"product_name\":\"A\"},{\"id\":2,\"product_name\":\"B\"}]}}");

            var page = await client.Products.SearchAsync(new ProductSearchRequest
            {
                ProductName = "A",
                Financial = new PriceFilter { Price = 1000, Operator = ">=" },
                Start = 0,
                Limit = 50
            });

            var q = http.Requests.Single(r => !r.IsTokenRequest).QueryPairs;
            Assert.Equal("A", q["product_name"]);
            Assert.Equal("1000", q["financial[price]"]);
            Assert.Equal(">=", q["financial[operator]"]);
            Assert.Equal("0", q["start"]);

            Assert.Equal(2, page.Meta.Total);
            Assert.Equal(0, page.Meta.Start);
            Assert.Equal(2, page.Products.Count);
        }

        [Fact]
        public async Task Search_WithoutFilters_SendsNoQueryString()
        {
            var (client, http) = TestClient.WithToken();
            http.Enqueue(HttpStatusCode.OK, "{\"status\":true,\"data\":{\"meta\":{},\"products\":[]}}");

            var page = await client.Products.SearchAsync();

            var request = http.Requests.Single(r => !r.IsTokenRequest);
            Assert.Equal(string.Empty, request.Query);
            Assert.Empty(page.Products);
        }

        [Fact]
        public void QueryStringBuilder_EncodesNestedObjectsListsBoolsAndEnums()
        {
            var query = QueryStringBuilder.Build(new
            {
                plain = "a b&c",
                flag = true,
                when = new DateTime(2026, 11, 7, 15, 30, 0),
                nested = new { inner = 5, deeper = new { leaf = "x" } },
                ids = new[] { 1, 2 },
                interval = Models.Enums.BillingInterval.Year,
                skipped = (string?)null
            });

            Assert.Equal(
                "?plain=a%20b%26c&flag=1&when=2026-11-07&nested%5Binner%5D=5&nested%5Bdeeper%5D%5Bleaf%5D=x&ids%5B0%5D=1&ids%5B1%5D=2&interval=4",
                query);
        }

        [Fact]
        public void QueryStringBuilder_ReturnsEmptyForNullOrEmptyObject()
        {
            Assert.Equal(string.Empty, QueryStringBuilder.Build(null));
            Assert.Equal(string.Empty, QueryStringBuilder.Build(new { a = (int?)null }));
        }
    }
}
