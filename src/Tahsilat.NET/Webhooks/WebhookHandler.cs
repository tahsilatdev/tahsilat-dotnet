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
        /// The signature header name sent by the Tahsilat server in webhook requests.
        /// Format: X-Tahsilat-Signature header contains t=timestamp,v1=signature
        /// </summary>
        public const string SignatureHeaderName = "X-Tahsilat-Signature";

        /// <summary>
        /// Constructs a webhook event from a string payload.
        /// Signature value is provided externally.
        /// </summary>
        /// <param name="payload">Request body (string)</param>
        /// <param name="signatureHeader">X-Tahsilat-Signature header value (t=...,v1=...)</param>
        /// <param name="webhookSecret">Webhook secret key from the dashboard</param>
        /// <param name="tolerance">Timestamp tolerance. If null, timestamp check is skipped. Default is 5 minutes.</param>
        public static WebhookEvent ConstructEvent(string payload, string signatureHeader, string webhookSecret, TimeSpan? tolerance = null)
        {
            // If tolerance is null, timestamp check is skipped.
            // For replay protection: ConstructEvent(payload, sig, secret, TimeSpan.FromMinutes(5))
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
        /// Constructs a webhook event from a raw byte[] payload.
        /// Signature value is provided externally.
        /// Prevents hash mismatches caused by encoding conversions.
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
        /// Constructs a webhook event from raw byte[] payload and a header dictionary (recommended usage).
        /// The SDK automatically extracts the X-Tahsilat-Signature header from the headers.
        /// The user does not need to know the header name.
        /// </summary>
        /// <param name="payloadBytes">HTTP request body (raw bytes)</param>
        /// <param name="headers">HTTP request headers (key-value)</param>
        /// <param name="webhookSecret">Webhook secret key from the dashboard</param>
        /// <param name="tolerance">Timestamp tolerance. If null, timestamp check is skipped. Default is 5 minutes.</param>
        public static WebhookEvent ConstructEvent(byte[] payloadBytes, IDictionary<string, string> headers, string webhookSecret, TimeSpan? tolerance = null)
        {
            string signatureHeader = null;

            if (headers != null)
            {
                // Case-insensitive header lookup
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
                    $"Missing {SignatureHeaderName} header. No signature found in the webhook request.");

            return ConstructEvent(payloadBytes, signatureHeader, webhookSecret, tolerance);
        }
    }
}
