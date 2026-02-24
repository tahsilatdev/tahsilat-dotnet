using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class RefundIntegrationTests : TestBase
    {
        [Fact]
        public async Task CreateRefund_ShouldReturnSuccessMessage()
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");

            long transactionId = 38142216687547;

            var req = new RefundCreateRequest
            {
                TransactionId = transactionId,
                Amount = 3000,
                Description = "Test iade işlemi."
            };

            var response = await tahsilat.Transactions.RefundAsync(req);

            Assert.NotNull(response);
            Assert.True(response.Status);
        }
    }
}
