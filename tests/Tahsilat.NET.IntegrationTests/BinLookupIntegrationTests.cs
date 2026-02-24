using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class BinLookupIntegrationTests : TestBase
    {
        [Fact]
        public async Task BinLookup_ShouldReturnBankData()
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            long bin_number = 48945540;

            var res = await tahsilat.BinLookup.DetailAsync(bin_number);

            Assert.NotNull(res);
            Assert.Equal(bin_number, res.BinNumber);

        }
    }
}
