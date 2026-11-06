using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Webhooks;

namespace Tahsilat.NET.Sample.Controllers
{
    public class WebhookController : Controller
    {
        private static readonly ConcurrentBag<(DateTime ReceivedAt, WebhookEvent Event)> _events = new();
        private readonly ILogger<WebhookController> _logger;

        private const string WebhookSecret = "whkey_secret_buraya";

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var events = _events.OrderByDescending(e => e.ReceivedAt).ToList();
            return View(events);
        }

        [HttpPost]
        public async Task<IActionResult> Receive()
        {
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);
            var payloadBytes = ms.ToArray();

            var signature = Request.Headers["X-Tahsilat-Signature"].FirstOrDefault() ?? string.Empty;

            try
            {
                // Varsayılan olarak 5 dakikadan eski webhook'lar reddedilir (replay koruması).
                var webhookEvent = WebhookHandler.ConstructEvent(payloadBytes, signature, WebhookSecret);

                if (webhookEvent.IsSuccess())
                {
                    _logger.LogInformation(
                        "Ödeme başarılı! Transaction: {TransactionId}, Tutar: {Amount} {Currency}",
                        webhookEvent.TransactionId, webhookEvent.Amount, webhookEvent.CurrencyCode);
                }
                else if (webhookEvent.IsFailed())
                {
                    _logger.LogWarning(
                        "Ödeme başarısız. Transaction: {TransactionId}",
                        webhookEvent.TransactionId);
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

                _events.Add((DateTime.Now, webhookEvent));

                return Ok("OK");
            }
            catch (TahsilatWebhookException ex)
            {
                _logger.LogWarning(ex, "Invalid webhook signature.");
                return BadRequest(new { error = "Invalid signature" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing error.");
                return Ok("OK");
            }
        }
    }
}