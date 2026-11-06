using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Tahsilat.NET.Infrastructure.Http
{
    /// <summary>
    /// Builds the query string of a GET request from a request object, using
    /// PHP-style bracket notation for nested values so the API's Laravel backend
    /// reads them as arrays: <c>customer_date[start_date]=2026-01-01</c>,
    /// <c>metadata[key]=source</c>.
    /// </summary>
    internal static class QueryStringBuilder
    {
        /// <summary>
        /// Returns "" when there is nothing to send, otherwise "?a=1&amp;b[c]=2"
        /// with every key and value percent-encoded.
        /// </summary>
        public static string Build(object? obj)
        {
            var pairs = new List<KeyValuePair<string, string>>();
            Append(null, obj, pairs);

            if (pairs.Count == 0)
                return string.Empty;

            var sb = new StringBuilder("?");
            for (int i = 0; i < pairs.Count; i++)
            {
                if (i > 0) sb.Append('&');
                sb.Append(Uri.EscapeDataString(pairs[i].Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(pairs[i].Value));
            }
            return sb.ToString();
        }

        private static void Append(string? prefix, object? value, List<KeyValuePair<string, string>> result)
        {
            if (value == null) return;

            if (IsScalar(value))
            {
                if (prefix == null)
                    throw new ArgumentException("A query string needs an object with properties, not a single value.", nameof(value));

                result.Add(new KeyValuePair<string, string>(prefix, ToQueryValue(value)));
                return;
            }

            if (value is IEnumerable list)
            {
                int index = 0;
                foreach (var item in list)
                {
                    Append($"{prefix}[{index}]", item, result);
                    index++;
                }
                return;
            }

            foreach (var prop in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;

                var propValue = prop.GetValue(value);
                if (propValue == null) continue;

                var name = prop.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? prop.Name;
                var key = prefix == null ? name : $"{prefix}[{name}]";

                Append(key, propValue, result);
            }
        }

        private static bool IsScalar(object value)
        {
            var type = value.GetType();
            return type.IsPrimitive
                || type.IsEnum
                || value is string
                || value is decimal
                || value is DateTime
                || value is DateTimeOffset;
        }

        private static string ToQueryValue(object value)
        {
            switch (value)
            {
                case bool b:
                    return b ? "1" : "0";

                // The API expects numeric codes for enum-typed fields.
                case Enum e:
                    return Convert.ToInt64(e, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

                // Date filters are calendar dates, not instants.
                case DateTime dt:
                    return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                case DateTimeOffset dto:
                    return dto.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                case IFormattable f:
                    return f.ToString(null, CultureInfo.InvariantCulture);

                default:
                    return value.ToString() ?? string.Empty;
            }
        }
    }
}
