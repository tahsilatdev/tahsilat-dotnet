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

        private const string WebhookSecret = "whsec_YOUR_WEBHOOK_SECRET";

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
            // 1. Request body'yi oku
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);
            var payloadBytes = ms.ToArray();

            // 2. Signature header'ını al
            var signature = Request.Headers["X-Tahsilat-Signature"].FirstOrDefault() ?? string.Empty;

            try
            {
                // 3. Webhook event'i doğrula ve parse et
                var webhookEvent = WebhookHandler.ConstructEvent(payloadBytes, signature, WebhookSecret);

                // 4. Ödeme durumuna göre işlem yap
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