using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Xunit;

namespace Tahsilat.NET.IntegrationTests
{
    public class ProductIntegrationTests : TestBase
    {
        [Fact]
        public async Task CreateProduct_ShouldReturnProduct()
        {

            var req = new ProductCreateRequest
            {
                ProductName = "Test Product",
                Price = 75900,
                Description = "Integration Test Product",
                Metadata = new()
                {
                    new Dictionary<string, object>
                    {
                        ["product_name"] = "Test Product",
                        ["product_type"] = "phone"
                    },
                    new Dictionary<string, object>
                    {
                        ["product_created"] = "Today",
                        ["source"] = "tahsilat-dotnet"
                    }
                }
            };

            var res = await Client.Products.CreateAsync(req);

            Assert.NotNull(res);
            Assert.True(res.Id > 0);
            Assert.False(string.IsNullOrEmpty(res.ProductName));

            // currency_id opsiyonel
            if (res.CurrencyId.HasValue)
                Assert.True(res.CurrencyId.Value > 0);

            // product_image_url opsiyonel — görsel yüklenmemişse null gelir
            if (!string.IsNullOrEmpty(res.ProductImageUrl))
                Assert.True(res.ProductImageUrl.Length > 0);

            Assert.False(res.IsRecurring);

            // payment_link bloğu create cevabında gelmiyor, yalnız arama satırlarında var.
            // Sandbox'ı kirletme
            await Client.Products.DeleteAsync(res.Id);
        }

        [Fact]
        public async Task CreateSubscriptionProduct_ShouldReturnRecurringFields()
        {
            var res = await Client.Products.CreateAsync(new ProductCreateRequest
            {
                ProductName = "Premium Üyelik (test)",
                Price = 49900,
                IsRecurring = true,
                BillingInterval = Tahsilat.NET.Models.Enums.BillingInterval.Month,
                IntervalCount = 1
            });

            try
            {
                Assert.True(res.IsRecurring);
                Assert.Equal((int)Tahsilat.NET.Models.Enums.BillingInterval.Month, res.BillingInterval);
                Assert.Equal(1, res.IntervalCount);
                Assert.False(string.IsNullOrEmpty(res.RecurringText)); // "Aylık"
            }
            finally
            {
                await Client.Products.DeleteAsync(res.Id);
            }
        }

        [Fact]
        public async Task SearchProduct_ById_ShouldReturnThatProduct()
        {
            var created = await Client.Products.CreateAsync(new ProductCreateRequest
            {
                ProductName = "Search Test Product",
                Price = 2500,
                Description = "Integration Test Product (search)"
            });

            try
            {
                // Arama cevabı liste değil { meta, products } nesnesi; parametreler
                // sorgu dizesinde gider. Arama indeksi create'in hemen ardından
                // güncel olmayabilir, kısa süre beklenir.
                Tahsilat.NET.Models.Responses.ProductSearchResponse page = null;
                var found = await EventuallyAsync(async () =>
                {
                    page = await Client.Products.SearchAsync(new ProductSearchRequest { ProductId = created.Id, Limit = 10 });
                    return page?.Products != null && page.Products.Any(p => p.Id == created.Id);
                });

                Assert.NotNull(page);
                Assert.NotNull(page.Meta);
                Assert.True(page.Meta.Total >= 1);
                Assert.True(found, $"Product {created.Id} was not returned by product_id search within the wait window.");
            }
            finally
            {
                await Client.Products.DeleteAsync(created.Id);
            }
        }

        [Fact]
        public async Task UpdateProductMetadata_ShouldReplaceAllEntries()
        {
            var created = await Client.Products.CreateAsync(new ProductCreateRequest
            {
                ProductName = "Metadata Test Product",
                Price = 1500,
                Metadata = new()
                {
                    new Dictionary<string, object> { ["category"] = "old" },
                    new Dictionary<string, object> { ["source"] = "tahsilat-dotnet" }
                }
            });

            // Liste mevcut metadata'nın tamamının yerine geçer: "source" gönderilmediği
            // için silinmeli, "category" yeni değeri almalı.
            var updated = await Client.Products.UpdateMetadataAsync(created.Id, new()
            {
                new Dictionary<string, object> { ["category"] = "new" }
            });

            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated.Id);
            Assert.NotNull(updated.Metadata);
            Assert.Single(updated.Metadata);
            Assert.Equal("category", updated.Metadata[0].Key);
            Assert.Equal("new", updated.Metadata[0].Value);

            await Client.Products.DeleteAsync(created.Id);
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnTrue()
        {
            var created = await Client.Products.CreateAsync(new ProductCreateRequest
            {
                ProductName = "Delete Test Product",
                Price = 1000,
                Description = "Integration Test Product (delete)"
            });

            Assert.NotNull(created);
            Assert.True(created.Id > 0);

            // DELETE isteği Content-Type başlığı olmadan 415 alıyordu.
            var deleted = await Client.Products.DeleteAsync(created.Id);

            Assert.True(deleted);
        }
    }
}
