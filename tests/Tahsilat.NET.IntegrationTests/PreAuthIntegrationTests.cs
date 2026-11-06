using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class PreAuthIntegrationTests : TestBase
    {
        [Fact]
        public async Task ResolvePreAuth_ShouldReturnSuccess()
        {

            long transactionId = 123154654658;

            var req = new PreAuthResolveRequest
            {
                TransactionId = transactionId,
                Status = true
            };

            var res = await Client.Transactions.ResolvePreAuthAsync(req);

            Assert.True(res.Status);
        }
    }
}
