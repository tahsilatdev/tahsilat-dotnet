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
                StockCode = "TEST",
                Category = "TEST",
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
        }
    }
}
