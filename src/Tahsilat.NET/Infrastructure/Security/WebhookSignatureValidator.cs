using System;
using System.Security.Cryptography;
using System.Text;

namespace Tahsilat.NET.Infrastructure.Security
{
    internal static class WebhookSignatureValidator
    {
        /// <summary>
        /// Default timestamp tolerance (replay protection): 5 minutes.
        /// </summary>
        public static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Parses the X-Tahsilat-Signature header.
        /// Format: t=timestamp,v1=signature
        /// </summary>
        /// <returns>Returns true on success; timestamp and signature out parameters are populated.</returns>
        public static bool ParseSignatureHeader(string header, out string timestamp, out string signature)
        {
            timestamp = null;
            signature = null;

            if (string.IsNullOrWhiteSpace(header))
                return false;

            var parts = header.Split(',');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("t=", StringComparison.OrdinalIgnoreCase))
                {
                    timestamp = trimmed.Substring(2);
                }
                else if (trimmed.StartsWith("v1=", StringComparison.OrdinalIgnoreCase))
                {
                    signature = trimmed.Substring(3);
                }
            }

            if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(signature))
                return false;

            return true;
        }

        /// <summary>
        /// Computes an HMAC-SHA256 signature.
        /// signedPayload = timestamp + "." + payload
        /// </summary>
        public static string ComputeSignature(string secret, string timestamp, string payload)
        {
            var signedPayload = timestamp + "." + payload;
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(payloadBytes);
                return BytesToHex(hash);
            }
        }

        /// <summary>
        /// Computes an HMAC-SHA256 signature (byte[] payload overload).
        /// signedPayload = timestamp_bytes + "." + payloadBytes
        /// </summary>
        public static string ComputeSignature(string secret, string timestamp, byte[] payloadBytes)
        {
            var timestampBytes = Encoding.UTF8.GetBytes(timestamp + ".");
            var signedPayload = new byte[timestampBytes.Length + payloadBytes.Length];
            Array.Copy(timestampBytes, 0, signedPayload, 0, timestampBytes.Length);
            Array.Copy(payloadBytes, 0, signedPayload, timestampBytes.Length, payloadBytes.Length);

            var keyBytes = Encoding.UTF8.GetBytes(secret);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(signedPayload);
                return BytesToHex(hash);
            }
        }

        /// <summary>
        /// Full webhook validation: parse → compute → compare → timestamp check.
        /// </summary>
        /// <param name="secretKey">Signing secret from the dashboard</param>
        /// <param name="payload">Request body (string)</param>
        /// <param name="signatureHeader">X-Tahsilat-Signature header value (t=...,v1=...)</param>
        /// <param name="tolerance">Timestamp tolerance. If null, timestamp check is skipped. Default is 5 minutes.</param>
        public static bool IsValid(string secretKey, string payload, string signatureHeader, TimeSpan? tolerance = null)
        {
            if (string.IsNullOrWhiteSpace(secretKey) ||
                string.IsNullOrWhiteSpace(signatureHeader))
                return false;

            string timestamp, signature;
            if (!ParseSignatureHeader(signatureHeader, out timestamp, out signature))
                return false;

            // Timestamp check (replay protection)
            if (tolerance.HasValue && !IsTimestampValid(timestamp, tolerance.Value))
                return false;

            var expected = ComputeSignature(secretKey, timestamp, payload);
            return FixedTimeEquals(expected, signature);
        }

        /// <summary>
        /// Full webhook validation: byte[] payload overload.
        /// </summary>
        public static bool IsValid(string secretKey, byte[] payloadBytes, string signatureHeader, TimeSpan? tolerance = null)
        {
            if (string.IsNullOrWhiteSpace(secretKey) ||
                string.IsNullOrWhiteSpace(signatureHeader) ||
                payloadBytes == null)
                return false;

            string timestamp, signature;
            if (!ParseSignatureHeader(signatureHeader, out timestamp, out signature))
                return false;

            // Timestamp check (replay protection)
            if (tolerance.HasValue && !IsTimestampValid(timestamp, tolerance.Value))
                return false;

            var expected = ComputeSignature(secretKey, timestamp, payloadBytes);
            return FixedTimeEquals(expected, signature);
        }

        /// <summary>
        /// Checks whether the Unix timestamp is within the specified tolerance.
        /// </summary>
        private static bool IsTimestampValid(string timestampStr, TimeSpan tolerance)
        {
            long timestamp;
            if (!long.TryParse(timestampStr, out timestamp))
                return false;

            var now = GetUnixTimeSeconds();
            var diff = Math.Abs(now - timestamp);
            return diff <= (long)tolerance.TotalSeconds;
        }

        /// <summary>
        /// Constant-time string comparison (timing attack protection).
        /// </summary>
        private static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null)
                return false;

            var aBytes = Encoding.UTF8.GetBytes(a);
            var bBytes = Encoding.UTF8.GetBytes(b);

#if NET6_0_OR_GREATER
            return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
#else
            int diff = aBytes.Length ^ bBytes.Length;
            int max = Math.Max(aBytes.Length, bBytes.Length);

            for (int i = 0; i < max; i++)
            {
                byte ba = i < aBytes.Length ? aBytes[i] : (byte)0;
                byte bb = i < bBytes.Length ? bBytes[i] : (byte)0;
                diff |= ba ^ bb;
            }

            return diff == 0;
#endif
        }

        /// <summary>
        /// Returns the current UTC time as a Unix timestamp (seconds).
        /// DateTimeOffset.ToUnixTimeSeconds() is not available in .NET 4.5.2.
        /// </summary>
        private static long GetUnixTimeSeconds()
        {
#if NET6_0_OR_GREATER
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
#elif NETSTANDARD2_0 || NETSTANDARD2_1
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
#else
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - epoch).TotalSeconds;
#endif
        }

        /// <summary>
        /// Converts a byte array to a lowercase hex string.
        /// </summary>
        private static string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
