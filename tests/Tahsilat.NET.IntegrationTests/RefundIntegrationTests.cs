using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Infrastructure.Http;
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

            // Sonuç yalnızca status + message olarak döner.
            // Endpoint "data" alanını doldurmaz — iade kaydının detayları için
            // işlemi Transactions.RetrieveAsync ile yeniden sorgulamak gerekir.
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.Null(response.Data);
        }

        /// <summary>
        /// Amount boş bırakıldığında (tam iade) istek gövdesine "amount" alanı
        /// hiç yazılmamalıdır — boş string ya da 0 olarak da gönderilmemelidir.
        /// </summary>
        [Fact]
        public async Task RefundRequest_WithNullAmount_ShouldNotWriteAmountToForm()
        {
            var req = new RefundCreateRequest
            {
                TransactionId = 38142216687547,
                Description = "Tam iade işlemi."
            };

            var form = await BuildFormAsync(req);

            Assert.False(form.ContainsKey("amount"));

            Assert.Equal("38142216687547", form["transaction_id"]);
            Assert.Equal("Tam iade işlemi.", form["description"]);
        }

        /// <summary>
        /// Amount dolu olduğunda (kısmi iade) mevcut davranış korunmalı:
        /// "amount" alanı kuruş cinsinden gövdeye yazılmalıdır.
        /// </summary>
        [Fact]
        public async Task RefundRequest_WithAmount_ShouldWriteAmountToForm()
        {
            var req = new RefundCreateRequest
            {
                TransactionId = 38142216687547,
                Amount = 3000,
                Description = "Kısmi iade işlemi."
            };

            var form = await BuildFormAsync(req);

            Assert.Equal("3000", form["amount"]);
            Assert.Equal("38142216687547", form["transaction_id"]);
            Assert.Equal("Kısmi iade işlemi.", form["description"]);
        }

        private static async Task<Dictionary<string, string>> BuildFormAsync(object request)
        {
            var content = FormUrlEncodedContentBuilder.Build(request);
            var raw = await content.ReadAsStringAsync();

            return raw
                .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split(new[] { '=' }, 2))
                .ToDictionary(
                    parts => Uri.UnescapeDataString(parts[0]),
                    parts => parts.Length > 1
                        ? Uri.UnescapeDataString(parts[1].Replace("+", " "))
                        : string.Empty);
        }
    }
}
