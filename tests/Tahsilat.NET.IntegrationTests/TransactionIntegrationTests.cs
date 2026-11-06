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
        /// <summary>
        /// Needs any existing sandbox transaction in Tahsilat:TransactionId.
        /// Without it the test returns.
        /// </summary>
        [Fact]
        public async Task RetrieveTransaction_ShouldReturnValidData()
        {
            if (KnownTransactionId == 0)
                return;

            var result = await Client.Transactions.RetrieveAsync(KnownTransactionId);

            Assert.NotNull(result);
            Assert.Equal(KnownTransactionId, result.TransactionId);
            Assert.True(result.Amount > 0);

            // charged_amount: komisyon müşteriye yansımadıysa null gelebilir
            if (result.ChargedAmount.HasValue)
            {
                Assert.True(result.ChargedAmount.Value >= 0);
                Assert.False(string.IsNullOrEmpty(result.FormattedChargedAmount));
            }

            // helper: charged_amount varsa onu, yoksa amount'u döner
            var decimal_ = result.GetChargedAmountDecimal();
            Assert.True(decimal_ > 0);
        }

        [Fact]
        public async Task RetrieveUnknownTransaction_ShouldThrowNotFound()
        {
            // Var olmayan 14 haneli bir ID: API 404 döner, SDK NotFound'a çevirir.
            await Assert.ThrowsAsync<Tahsilat.NET.Exceptions.TahsilatNotFoundException>(
                () => Client.Transactions.RetrieveAsync(10000000000001));
        }
    }
}
