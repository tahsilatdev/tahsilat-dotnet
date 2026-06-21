using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class CommissionIntegrationTests : TestBase
    {
        [Fact]
        public async Task GetCommissions_ShouldReturnList()
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var req = new CommissionSearchRequest
            {
                BinNumber = 1234567
            };

            var res = await tahsilat.Commissions.SearchAsync(req);

            Assert.NotNull(res);
            Assert.True(res.Count > 0);

            var first = res.First();
            Assert.True(first.Id > 0);
            Assert.True(first.CommissionRate >= 0);

            // transaction_fee opsiyonel — varsa negatif olmamalı
            if (first.TransactionFee.HasValue)
                Assert.True(first.TransactionFee.Value >= 0);
        }
    }
}
