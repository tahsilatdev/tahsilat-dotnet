using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Webhooks;

namespace Tahsilat.NET.Sample.Controllers
{
    public class WebhookController : Controller
    {
        public sealed record ReceivedWebhook(DateTime ReceivedAt, WebhookEvent Event, string RawPayload);

        private static readonly ConcurrentBag<ReceivedWebhook> _events = new();
        private readonly ILogger<WebhookController> _logger;
        private readonly string _webhookSecret;

        public WebhookController(ILogger<WebhookController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _webhookSecret = configuration["Tahsilat:WebhookSecret"] ?? string.Empty;
        }

        public IActionResult Index()
        {
            ViewBag.SecretConfigured = !string.IsNullOrEmpty(_webhookSecret) && !_webhookSecret.Contains("YOUR_") && !_webhookSecret.Contains("BURAYA");
            return View(_events.OrderByDescending(e => e.ReceivedAt).ToList());
        }

        /// <summary>
        /// Tahsilat panelinde webhook adresi olarak {ngrok-url}/webhook/receive tanımlayın.
        /// Harici bir POST olduğu için CSRF korumasından muaftır.
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Receive()
        {
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);
            var payloadBytes = ms.ToArray();

            // Başlık adını bilmek gerekmez: SDK sözlükten X-Tahsilat-Signature'ı kendisi bulur.
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            try
            {
                // Replay koruması varsayılan olarak açık: 5 dakikadan eski webhook'lar reddedilir.
                var webhookEvent = WebhookHandler.ConstructEvent(payloadBytes, headers, _webhookSecret);

                if (webhookEvent.IsSuccess())
                {
                    _logger.LogInformation(
                        "Ödeme başarılı! Transaction: {TransactionId}, Tutar: {Amount} {Currency}",
                        webhookEvent.TransactionId, webhookEvent.FormattedAmount, webhookEvent.CurrencyCode);
                }
                else if (webhookEvent.IsFailed())
                {
                    _logger.LogWarning("Ödeme başarısız. Transaction: {TransactionId}", webhookEvent.TransactionId);
                }

                // Abonelik ödemelerinde "subscription" bloğu dolu gelir; dönem tahsilatları
                // yeni bir transaction_id ile ayrı işlem olarak düşer (cycle_number >= 1).
                if (webhookEvent.HasSubscription())
                {
                    _logger.LogInformation(
                        "Abonelik ödemesi. Subscription: {SubscriptionId}, Dönem: {Cycle}, İlk ödeme: {IsInitial}, Durum: {Status}",
                        webhookEvent.GetSubscriptionId(),
                        webhookEvent.GetSubscriptionCycleNumber(),
                        webhookEvent.IsInitialSubscriptionPayment(),
                        webhookEvent.GetSubscriptionStatusTextTr());
                }

                _events.Add(new ReceivedWebhook(DateTime.Now, webhookEvent, Encoding.UTF8.GetString(payloadBytes)));

                return Ok("OK");
            }
            catch (TahsilatWebhookException ex)
            {
                // İmza yanlış ya da zaman damgası tolerans dışında.
                _logger.LogWarning(ex, "Webhook reddedildi: {Reason}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing error.");
                return Ok("OK");
            }
        }
    }
}
