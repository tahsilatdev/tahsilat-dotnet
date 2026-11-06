using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class CustomerIntegrationTests : TestBase
    {
        [Fact]
        public async Task CreateCustomer_ShouldReturnCustomerData()
        {

            var req = new CustomerCreateRequest
            {
                Name = "Test",
                LastName = "User",
                Email = "testuser@mail.com",
                Phone = "5321234567",
                Country = "TR",
                Metadata = new()
                {
                    new Dictionary<string, object>
                    {
                        ["customer_name"] = "testuser",
                        ["customer_type"] = "premium"
                    },
                    new Dictionary<string, object>
                    {
                        ["customer_created"] = "Today",
                        ["source"] = "tahsilat-dotnet"
                    }
                }
            };

            var result = await Client.Customers.CreateAsync(req);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Test", result.Name);

            // ip ve formatted_updated_at opsiyonel
            if (!string.IsNullOrEmpty(result.Ip))
                Assert.True(result.Ip.Length > 0);

            if (!string.IsNullOrEmpty(result.FormattedUpdatedAt))
                Assert.True(result.FormattedUpdatedAt.Length > 0);

            // payment_link bloğu create cevabında gelmiyor, yalnız arama satırlarında var.
            // Sandbox'ı kirletme
            await Client.Customers.DeleteAsync(result.Id);
        }

        [Fact]
        public async Task SearchCustomer_ByEmail_ShouldReturnThatCustomer()
        {
            var email = $"search{DateTime.UtcNow.Ticks}@mail.com";

            var created = await Client.Customers.CreateAsync(new CustomerCreateRequest
            {
                Name = "Search",
                LastName = "Test",
                Email = email,
                Phone = "5321234567",
                Country = "TR"
            });

            try
            {
                // Arama cevabı liste değil { meta, customers } nesnesi; parametreler
                // sorgu dizesinde gider ve URL-encode edilir. Arama indeksi create'in
                // hemen ardından güncel olmayabilir, kısa süre beklenir.
                CustomerSearchResponse page = null;
                var found = await EventuallyAsync(async () =>
                {
                    page = await Client.Customers.SearchAsync(new CustomerSearchRequest { Email = email, Limit = 10 });
                    return page?.Customers != null && page.Customers.Any(c => c.Id == created.Id);
                });

                Assert.NotNull(page);
                Assert.NotNull(page.Meta);
                Assert.True(page.Meta.Total >= 1);
                Assert.True(found, $"Customer {created.Id} was not returned by email search within the wait window.");
            }
            finally
            {
                await Client.Customers.DeleteAsync(created.Id);
            }
        }

        [Fact]
        public async Task DeleteCustomer_ShouldReturnTrue()
        {
            var created = await Client.Customers.CreateAsync(new CustomerCreateRequest
            {
                Name = "Delete",
                LastName = "Test",
                Email = "deletetest@mail.com",
                Phone = "5321234567",
                Country = "TR"
            });

            Assert.NotNull(created);
            Assert.True(created.Id > 0);

            // DELETE isteği Content-Type başlığı olmadan 415 alıyordu.
            var deleted = await Client.Customers.DeleteAsync(created.Id);

            Assert.True(deleted);
        }
    }
}
