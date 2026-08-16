using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;
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

        [Fact]
        public async Task GetCommissions_ShouldExposeCardDimensionFields()
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");

            // BIN gönderilmediğinde üye işyerinin tüm aktif POS'larının oranları döner
            var res = await tahsilat.Commissions.SearchAsync();

            Assert.NotNull(res);
            Assert.True(res.Count > 0);

            foreach (var c in res)
            {
                // card_type null olabilir (= tüm kart türleri), doluysa bilinen bir değer olmalı
                if (c.CardType != null)
                    Assert.Contains(c.CardType, new[] { CardTypes.Credit, CardTypes.Debit, CardTypes.Prepaid });

                // installment_text her satırda dolu gelmeli
                Assert.False(string.IsNullOrEmpty(c.InstallmentText));
            }

            // Bir satır installment + card_type + is_on_us + is_foreign ile tekilleşir
            var duplicates = res
                .GroupBy(c => new { c.PosId, c.Installment, c.CardType, c.IsOnUs, c.IsForeign })
                .Where(g => g.Count() > 1)
                .ToList();

            Assert.Empty(duplicates);
        }

        [Fact]
        public void Deserialize_ShouldTolerateNullCardDimensionFields()
        {
            // card_type  = null -> tüm kart türleri
            // is_on_us   = null -> hem on-us hem not-on-us
            // is_foreign = null -> eski/eksik veri; non-nullable bool'a düşmemeli, false kalmalı
            const string json = @"{
                ""status"": true,
                ""data"": [{
                    ""id"": 1,
                    ""merchant_id"": 1234567,
                    ""company_pos_credential_id"": null,
                    ""pos_id"": null,
                    ""pos_name"": null,
                    ""installment"": 1,
                    ""installment_text"": ""Tek çekim"",
                    ""card_type"": null,
                    ""is_on_us"": null,
                    ""is_foreign"": null,
                    ""commission_rate"": 3.0,
                    ""commission_by"": 1,
                    ""commission_by_text"": ""Üye İşyeri""
                }]
            }";

            var response = JsonConvert.DeserializeObject<ApiResponse<List<CommissionResponse>>>(json);

            Assert.NotNull(response);
            var row = Assert.Single(response.Data);

            Assert.Null(row.CompanyPosCredentialId);
            Assert.Null(row.PosId);
            Assert.Null(row.PosName);
            Assert.Null(row.CardType);
            Assert.Null(row.IsOnUs);
            Assert.False(row.IsForeign);
            Assert.Equal("Tek çekim", row.InstallmentText);
        }

        [Fact]
        public void Deserialize_ShouldMapCardDimensionFields()
        {
            const string json = @"{
                ""status"": true,
                ""data"": [{
                    ""merchant_id"": 1234567,
                    ""company_pos_credential_id"": 4,
                    ""pos_id"": 4,
                    ""pos_name"": ""Ziraat Pay Pos"",
                    ""installment"": 1,
                    ""installment_text"": ""Tek çekim"",
                    ""card_type"": ""credit"",
                    ""is_on_us"": false,
                    ""is_foreign"": false,
                    ""commission_rate"": 3.0,
                    ""commission_by"": 1,
                    ""commission_by_text"": ""Üye İşyeri"",
                    ""created_at"": ""2025-06-03T21:27:18+03:00"",
                    ""updated_at"": ""2025-06-03T21:27:18+03:00"",
                    ""card_family"": { ""name"": ""Axess"", ""slug"": ""axess"", ""logo_url"": ""https://example.com/axess.webp"" },
                    ""card_segment_type"": { ""name"": ""Consumer"", ""slug"": ""consumer"" }
                }]
            }";

            var response = JsonConvert.DeserializeObject<ApiResponse<List<CommissionResponse>>>(json);

            var row = Assert.Single(response.Data);

            Assert.Equal(4, row.CompanyPosCredentialId);
            Assert.Equal(4, row.PosId);
            Assert.Equal("Ziraat Pay Pos", row.PosName);
            Assert.Equal(CardTypes.Credit, row.CardType);
            Assert.False(row.IsOnUs);
            Assert.False(row.IsForeign);
            Assert.Equal(3.0m, row.CommissionRate);
            Assert.Equal("Axess", row.CardFamily.Name);
            Assert.Equal("consumer", row.CardSegmentType.Slug);
        }
    }
}
