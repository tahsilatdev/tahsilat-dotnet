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
    public class PaymentIntegrationTests : TestBase
    {
        [Fact]
        public async Task Create3dsPayment_ShouldReturnPaymentPageUrl()
        {
            var productRequest = new ProductCreateRequest
            {
                ProductName = "Test Ürünü",
                Price = 5000,
                Description = "Entegrasyon Test"
            };

            var productResponse = await Client.Products.CreateAsync(productRequest);

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

            try
            {
                var response = await Client.Payments.CreateAsync(paymentRequest);

                Assert.NotNull(response);
                Assert.True(response.TransactionId > 0);
                Assert.False(string.IsNullOrEmpty(response.PaymentPageUrl));
            }
            finally
            {
                // Ödeme sayfası oluşturuldu ama ödenmedi; test ürününü sandbox'ta bırakma.
                await Client.Products.DeleteAsync(productResponse.Id);
            }
        }
    }
}
