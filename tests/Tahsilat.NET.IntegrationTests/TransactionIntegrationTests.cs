using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class TransactionIntegrationTests : TestBase
    {
        [Fact]
        public async Task RetrieveTransaction_ShouldReturnValidData()
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            long exampleTrxId = 83113981717821;

            var result = await tahsilat.Transactions.RetrieveAsync(exampleTrxId);

            Assert.NotNull(result);
            Assert.Equal(exampleTrxId, result.TransactionId);
        }
    }
}
