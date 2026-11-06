using Newtonsoft.Json;
using Tahsilat.NET.Extensions;
using Tahsilat.NET.Models.Enums;
using Tahsilat.NET.Models.Responses;
using Tahsilat.NET.Webhooks;
using Xunit;

namespace Tahsilat.NET.Tests
{
    public class SubscriptionFieldsTests
    {
        private const string RecurringCycle =
            "{\"transaction_id\":75690998341504,\"amount\":49900,\"formatted_amount\":\"499.00\"," +
            "\"payment_status\":1,\"transaction_status\":2," +
            "\"subscription\":{\"subscription_id\":19464539815644,\"status\":1,\"cycle_number\":1," +
            "\"next_billing_at\":\"2027-01-05T09:00:03+03:00\",\"cancelled_at\":null,\"status_text\":\"active\",\"is_initial_payment\":false}}";

        [Fact]
        public void TransactionResult_ReadsSubscriptionBlock()
        {
            var result = JsonConvert.DeserializeObject<TransactionResult>(RecurringCycle)!;

            Assert.True(result.HasSubscription());
            Assert.Equal(19464539815644, result.GetSubscriptionId());
            Assert.Equal(1, result.GetSubscriptionCycleNumber());
            Assert.False(result.IsInitialSubscriptionPayment());
            Assert.Equal("Aktif", result.GetSubscriptionStatusTextTr());
            Assert.Equal("active", result.Subscription.StatusText);
            Assert.Equal((int)SubscriptionStatus.Active, result.Subscription.Status);
            Assert.Equal("2027-01-05T09:00:03+03:00", result.Subscription.NextBillingAt);
            Assert.Null(result.Subscription.CancelledAt);
            Assert.True(result.IsSuccess());
        }

        [Fact]
        public void WebhookEvent_ReadsSubscriptionBlockAndFormattedAmount()
        {
            var evt = JsonConvert.DeserializeObject<WebhookEvent>(RecurringCycle)!;

            Assert.True(evt.HasSubscription());
            Assert.Equal(19464539815644, evt.GetSubscriptionId());
            Assert.Equal(1, evt.GetSubscriptionCycleNumber());
            Assert.Equal("Aktif", evt.GetSubscriptionStatusTextTr());
            Assert.Equal("499.00", evt.FormattedAmount);
        }

        [Fact]
        public void InitialPayment_IsCycleZero()
        {
            var evt = JsonConvert.DeserializeObject<WebhookEvent>(
                "{\"transaction_id\":1,\"subscription\":{\"subscription_id\":7,\"status\":2,\"cycle_number\":0,\"is_initial_payment\":true}}")!;

            Assert.True(evt.IsInitialSubscriptionPayment());
            Assert.Equal(0, evt.GetSubscriptionCycleNumber());
            Assert.Equal("Ödeme Bekliyor", evt.GetSubscriptionStatusTextTr());
        }

        [Theory]
        [InlineData("{\"transaction_id\":1,\"subscription\":null}")]
        [InlineData("{\"transaction_id\":1}")]
        public void NullOrMissingSubscription_MeansNotASubscriptionPayment(string json)
        {
            var result = JsonConvert.DeserializeObject<TransactionResult>(json)!;
            var evt = JsonConvert.DeserializeObject<WebhookEvent>(json)!;

            Assert.False(result.HasSubscription());
            Assert.Null(result.Subscription);
            Assert.Null(result.GetSubscriptionId());
            Assert.Null(result.GetSubscriptionCycleNumber());
            Assert.False(result.IsInitialSubscriptionPayment());
            Assert.Null(result.GetSubscriptionStatusTextTr());

            Assert.False(evt.HasSubscription());
            Assert.Null(evt.GetSubscriptionStatusTextTr());
        }

        [Theory]
        [InlineData(1, "Aktif")]
        [InlineData(2, "Ödeme Bekliyor")]
        [InlineData(3, "İptal Edildi")]
        [InlineData(4, "Tamamlandı")]
        [InlineData(5, "İlk Ödeme Bekleniyor")]
        [InlineData(99, "Bilinmiyor")]
        public void SubscriptionStatus_TurkishLabels(int status, string expected)
        {
            var result = JsonConvert.DeserializeObject<TransactionResult>(
                "{\"transaction_id\":1,\"subscription\":{\"subscription_id\":7,\"status\":" + status + "}}")!;

            Assert.Equal(expected, result.GetSubscriptionStatusTextTr());
        }

        [Fact]
        public void ProductResponse_ReadsRecurringFieldsAndPaymentLink()
        {
            var product = JsonConvert.DeserializeObject<ProductResponse>(
                "{\"id\":55195540441484,\"product_name\":\"Premium Üyelik\",\"price\":49900," +
                "\"is_recurring\":true,\"billing_interval\":3,\"interval_count\":1,\"recurring_text\":\"Aylık\"," +
                "\"payment_link\":\"https://pay.tahsilat.com/x\"}")!;

            Assert.True(product.IsRecurring);
            Assert.Equal((int)BillingInterval.Month, product.BillingInterval);
            Assert.Equal(1, product.IntervalCount);
            Assert.Equal("Aylık", product.RecurringText);
            Assert.Equal("https://pay.tahsilat.com/x", product.PaymentLink);
        }

        [Fact]
        public void OneOffProduct_HasNullRecurringFields()
        {
            var product = JsonConvert.DeserializeObject<ProductResponse>("{\"id\":1,\"product_name\":\"Tek seferlik\",\"price\":1000}")!;

            Assert.False(product.IsRecurring);
            Assert.Null(product.BillingInterval);
            Assert.Null(product.IntervalCount);
            Assert.Null(product.RecurringText);
        }
    }
}
