using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Infrastructure.Security;

namespace Tahsilat.NET.Webhooks
{
    public static class WebhookHandler
    {
        /// <summary>
        /// Tahsilat sunucusunun webhook isteğinde gönderdiği signature header adı.
        /// Yeni format: X-Tahsilat-Signature header'ında t=timestamp,v1=signature
        /// </summary>
        public const string SignatureHeaderName = "X-Tahsilat-Signature";

        /// <summary>
        /// String payload ile webhook event oluşturur.
        /// Signature değerini dışarıdan alır.
        /// </summary>
        /// <param name="payload">Request body (string)</param>
        /// <param name="signatureHeader">X-Tahsilat-Signature header değeri (t=...,v1=...)</param>
        /// <param name="webhookSecret">Panelden alınan webhook secret key</param>
        /// <param name="tolerance">Timestamp toleransı. null geçilirse timestamp kontrolü yapılmaz. Varsayılan 5 dakika.</param>
        public static WebhookEvent ConstructEvent(string payload, string signatureHeader, string webhookSecret, TimeSpan? tolerance = null)
        {
            // tolerance parametresi verilmediyse (null), timestamp kontrolü yapılmaz.
            // Replay koruması isteniyorsa: ConstructEvent(payload, sig, secret, TimeSpan.FromMinutes(5))
            if (string.IsNullOrWhiteSpace(signatureHeader))
                throw new TahsilatWebhookException("Missing X-Tahsilat-Signature header.");

            if (!WebhookSignatureValidator.IsValid(webhookSecret, payload, signatureHeader, tolerance))
                throw new TahsilatWebhookException("Invalid webhook signature.");

            var evt = JsonConvert.DeserializeObject<WebhookEvent>(payload);

            if (evt == null)
                throw new TahsilatWebhookException("Invalid webhook payload.");

            return evt;
        }

        /// <summary>
        /// Raw byte[] payload ile webhook event oluşturur.
        /// Signature değerini dışarıdan alır.
        /// Encoding dönüşümü kaynaklı hash uyumsuzluğunu önler.
        /// </summary>
        public static WebhookEvent ConstructEvent(byte[] payloadBytes, string signatureHeader, string webhookSecret, TimeSpan? tolerance = null)
        {
            if (string.IsNullOrWhiteSpace(signatureHeader))
                throw new TahsilatWebhookException("Missing X-Tahsilat-Signature header.");

            if (!WebhookSignatureValidator.IsValid(webhookSecret, payloadBytes, signatureHeader, tolerance))
                throw new TahsilatWebhookException("Invalid webhook signature.");

            var payloadString = Encoding.UTF8.GetString(payloadBytes);
            var evt = JsonConvert.DeserializeObject<WebhookEvent>(payloadString);

            if (evt == null)
                throw new TahsilatWebhookException("Invalid webhook payload.");

            return evt;
        }

        /// <summary>
        /// Raw byte[] payload ve header dictionary ile webhook event oluşturur (önerilen kullanım).
        /// SDK, X-Tahsilat-Signature header'ını otomatik olarak headers içinden çeker.
        /// Kullanıcının header adını bilmesine gerek yoktur.
        /// </summary>
        /// <param name="payloadBytes">HTTP request body (raw bytes)</param>
        /// <param name="headers">HTTP request header'ları (key-value)</param>
        /// <param name="webhookSecret">Panelden alınan webhook secret key</param>
        /// <param name="tolerance">Timestamp toleransı. null geçilirse timestamp kontrolü yapılmaz. Varsayılan 5 dakika.</param>
        public static WebhookEvent ConstructEvent(byte[] payloadBytes, IDictionary<string, string> headers, string webhookSecret, TimeSpan? tolerance = null)
        {
            string signatureHeader = null;

            if (headers != null)
            {
                // Büyük-küçük harf duyarsız arama
                foreach (var kvp in headers)
                {
                    if (string.Equals(kvp.Key, SignatureHeaderName, StringComparison.OrdinalIgnoreCase))
                    {
                        signatureHeader = kvp.Value;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(signatureHeader))
                throw new TahsilatWebhookException(
                    $"Missing {SignatureHeaderName} header. Webhook isteğinde signature bulunamadı.");

            return ConstructEvent(payloadBytes, signatureHeader, webhookSecret, tolerance);
        }
    }
}
