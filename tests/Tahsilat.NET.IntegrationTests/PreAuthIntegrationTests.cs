using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class PreAuthIntegrationTests : TestBase
    {
        /// <summary>
        /// Needs an open pre-authorisation: pay with PreAuth = true on the sandbox,
        /// then put its 14-digit transaction_id into Tahsilat:PreAuthTransactionId.
        /// Without it the test has nothing to resolve and returns.
        /// </summary>
        [Fact]
        public async Task ResolvePreAuth_ShouldReturnSuccess()
        {
            if (PreAuthTransactionId == 0)
                return;

            var res = await Client.Transactions.ResolvePreAuthAsync(new PreAuthResolveRequest
            {
                TransactionId = PreAuthTransactionId,
                Status = true
            });

            Assert.True(res.Status);
            Assert.False(string.IsNullOrEmpty(res.Message));
        }
    }
}
