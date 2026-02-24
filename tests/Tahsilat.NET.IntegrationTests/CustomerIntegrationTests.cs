using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class CustomerIntegrationTests : TestBase
    {
        [Fact]
        public async Task CreateCustomer_ShouldReturnCustomerData()
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");

            var req = new CustomerCreateRequest
            {
                Name = "Test",
                LastName = "User",
                Email = "testuser@mail.com",
                Phone = "+901234567890",
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

            var result = await tahsilat.Customers.CreateAsync(req);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Name", result.Name);
        }
    }
}
