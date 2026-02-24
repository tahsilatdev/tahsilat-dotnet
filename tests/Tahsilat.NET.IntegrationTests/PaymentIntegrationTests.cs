using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class PaymentIntegrationTests
    {
        [Fact]
        public async Task Create3dsPayment_ShouldReturnPaymentPageUrl()
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");

            var productRequest = new ProductCreateRequest
            {
                ProductName = "Test Ürünü",
                Price = 5000,
                Description = "Entegrasyon Test"
            };

            var productResponse = await tahsilat.Products.CreateAsync(productRequest);

            var paymentRequest = new PaymentCreateRequest
            {
                Currency = "TRY",
                Amount = 5000,
                ProductIds = new List<long>
                {
                    productResponse.Id
                },
                Metadata = new()
                {
                    new Dictionary<string, object>
                    {
                        ["order_id"] = 1234,
                        ["customer_type"] = "premium"
                    },
                    new Dictionary<string, object>
                    {
                        ["test"] = "asd-2026",
                        ["asd"] = "asdtahsilat-dotnet"
                    }
                }
            };

            var response = await tahsilat.Payments.CreateAsync(paymentRequest);

            Assert.NotNull(response);
            Assert.True(response.TransactionId > 0);
            Assert.False(string.IsNullOrEmpty(response.PaymentPageUrl));
        }
    }
}
