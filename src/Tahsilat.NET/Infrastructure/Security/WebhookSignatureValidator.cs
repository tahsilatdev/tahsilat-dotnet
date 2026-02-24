using System;
using System.Security.Cryptography;
using System.Text;

namespace Tahsilat.NET.Infrastructure.Security
{
    internal static class WebhookSignatureValidator
    {
        /// <summary>
        /// Varsayılan timestamp toleransı (replay koruması): 5 dakika.
        /// </summary>
        public static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(5);

        /// <summary>
        /// X-Tahsilat-Signature header'ını parse eder.
        /// Format: t=timestamp,v1=signature
        /// </summary>
        /// <returns>Başarılı ise true, timestamp ve signature out parametreleri dolar.</returns>
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
        /// HMAC-SHA256 imzası hesaplar.
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
        /// HMAC-SHA256 imzası hesaplar (byte[] payload overload).
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
        /// Tam webhook doğrulaması: parse → compute → compare → timestamp check.
        /// </summary>
        /// <param name="secretKey">Panelden alınan signing secret</param>
        /// <param name="payload">Request body (string)</param>
        /// <param name="signatureHeader">X-Tahsilat-Signature header değeri (t=...,v1=...)</param>
        /// <param name="tolerance">Timestamp toleransı. null ise timestamp kontrolü yapılmaz. Varsayılan 5 dakika.</param>
        public static bool IsValid(string secretKey, string payload, string signatureHeader, TimeSpan? tolerance = null)
        {
            if (string.IsNullOrWhiteSpace(secretKey) ||
                string.IsNullOrWhiteSpace(signatureHeader))
                return false;

            string timestamp, signature;
            if (!ParseSignatureHeader(signatureHeader, out timestamp, out signature))
                return false;

            // Timestamp kontrolü (replay koruması)
            if (tolerance.HasValue && !IsTimestampValid(timestamp, tolerance.Value))
                return false;

            var expected = ComputeSignature(secretKey, timestamp, payload);
            return FixedTimeEquals(expected, signature);
        }

        /// <summary>
        /// Tam webhook doğrulaması: byte[] payload overload.
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

            // Timestamp kontrolü (replay koruması)
            if (tolerance.HasValue && !IsTimestampValid(timestamp, tolerance.Value))
                return false;

            var expected = ComputeSignature(secretKey, timestamp, payloadBytes);
            return FixedTimeEquals(expected, signature);
        }

        /// <summary>
        /// Unix timestamp'ın belirtilen tolerans içinde olup olmadığını kontrol eder.
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
        /// Constant-time string karşılaştırma (timing attack koruması).
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
        /// Şu anki UTC zamanını Unix timestamp (saniye) olarak döndürür.
        /// .NET 4.5.2'de DateTimeOffset.ToUnixTimeSeconds() yoktur.
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
        /// Byte dizisini lowercase hex string'e çevirir.
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
