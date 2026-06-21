using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class ProductIntegrationTests : TestBase
    {
        [Fact]
        public async Task CreateProduct_ShouldReturnProduct()
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");

            var req = new ProductCreateRequest
            {
                ProductName = "Test Product",
                Price = 75900,
                Description = "Integration Test Product",
                Metadata = new()
                {
                    new Dictionary<string, object>
                    {
                        ["product_name"] = "Test Product",
                        ["product_type"] = "phone"
                    },
                    new Dictionary<string, object>
                    {
                        ["product_created"] = "Today",
                        ["source"] = "tahsilat-dotnet"
                    }
                }
            };

            var res = await tahsilat.Products.CreateAsync(req);

            Assert.NotNull(res);
            Assert.True(res.Id > 0);
            Assert.False(string.IsNullOrEmpty(res.ProductName));

            // currency_id opsiyonel
            if (res.CurrencyId.HasValue)
                Assert.True(res.CurrencyId.Value > 0);

            // product_image_url opsiyonel — görsel yüklenmemişse null gelir
            if (!string.IsNullOrEmpty(res.ProductImageUrl))
                Assert.True(res.ProductImageUrl.Length > 0);
        }
    }
}
