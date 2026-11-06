using System.Security.Cryptography;
using System.Text;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Webhooks;
using Xunit;

namespace Tahsilat.NET.Tests
{
    public class WebhookTests
    {
        private const string Secret = "whsec_unit";
        private const string Payload = "{\"transaction_id\":1,\"payment_status\":1,\"transaction_status\":2,\"amount\":100}";

        private static string Sign(string payload, long timestamp)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp + "." + payload));
            return "t=" + timestamp + ",v1=" + Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [Fact]
        public void FreshWebhook_WithValidSignature_IsAccepted()
        {
            var evt = WebhookHandler.ConstructEvent(Payload, Sign(Payload, Now()), Secret);

            Assert.Equal(1, evt.TransactionId);
            Assert.True(evt.IsSuccess());
        }

        [Fact]
        public void OldWebhook_IsRejectedByDefault()
        {
            var tenMinutesAgo = Now() - 600;

            var ex = Assert.Throws<TahsilatWebhookException>(() =>
                WebhookHandler.ConstructEvent(Payload, Sign(Payload, tenMinutesAgo), Secret));

            Assert.Contains("Invalid webhook signature", ex.Message);
        }

        [Fact]
        public void FutureWebhook_IsRejectedByDefault()
        {
            var tenMinutesAhead = Now() + 600;

            Assert.Throws<TahsilatWebhookException>(() =>
                WebhookHandler.ConstructEvent(Payload, Sign(Payload, tenMinutesAhead), Secret));
        }

        [Fact]
        public void OldWebhook_WithinExplicitTolerance_IsAccepted()
        {
            var tenMinutesAgo = Now() - 600;

            var evt = WebhookHandler.ConstructEvent(Payload, Sign(Payload, tenMinutesAgo), Secret, TimeSpan.FromMinutes(15));

            Assert.Equal(1, evt.TransactionId);
        }

        [Fact]
        public void OldWebhook_WithTimestampCheckDisabled_IsAccepted()
        {
            var longAgo = Now() - 86400;

            var evt = WebhookHandler.ConstructEvent(Payload, Sign(Payload, longAgo), Secret, WebhookHandler.DisableTimestampCheck);

            Assert.Equal(1, evt.TransactionId);
        }

        [Fact]
        public void TamperedPayload_IsRejectedEvenWithTimestampCheckDisabled()
        {
            var header = Sign(Payload, Now());
            var tampered = Payload.Replace("\"amount\":100", "\"amount\":1");

            Assert.Throws<TahsilatWebhookException>(() =>
                WebhookHandler.ConstructEvent(tampered, header, Secret, WebhookHandler.DisableTimestampCheck));
        }

        [Fact]
        public void HeaderDictionaryOverload_FindsSignatureCaseInsensitively()
        {
            var headers = new Dictionary<string, string> { ["x-tahsilat-signature"] = Sign(Payload, Now()) };

            var evt = WebhookHandler.ConstructEvent(Encoding.UTF8.GetBytes(Payload), headers, Secret);

            Assert.Equal(1, evt.TransactionId);
        }

        [Fact]
        public void MissingHeader_Throws()
        {
            Assert.Throws<TahsilatWebhookException>(() => WebhookHandler.ConstructEvent(Payload, "", Secret));
        }

        [Fact]
        public void DefaultTolerance_IsFiveMinutes()
        {
            Assert.Equal(TimeSpan.FromMinutes(5), WebhookHandler.DefaultTolerance);
        }
    }
}
