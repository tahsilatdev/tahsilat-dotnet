using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Tahsilat.NET.Infrastructure.Http
{
    /// <summary>
    /// Converts objects to FormUrlEncodedContent.
    /// Complex objects (lists, dictionaries) are serialized as JSON strings.
    /// </summary>
    internal static class FormUrlEncodedContentBuilder
    {
        private static readonly HashSet<string> JsonStringFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "products",
        };

        private static readonly Regex MetadataValueInvalidChars = new Regex(@"[^a-zA-Z0-9 ._]", RegexOptions.Compiled);

        public static FormUrlEncodedContent Build(object obj)
        {
            var keyValuePairs = new List<KeyValuePair<string, string>>();
            ProcessObject(obj, keyValuePairs);
            return new FormUrlEncodedContent(keyValuePairs);
        }

        private static void ProcessObject(object obj, List<KeyValuePair<string, string>> result)
        {
            if (obj == null) return;

            var type = obj.GetType();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead) continue;

                var value = prop.GetValue(obj);
                if (value == null) continue;

                var jsonAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                var propName = jsonAttr?.PropertyName ?? prop.Name;

                if (propName.Equals("metadata", StringComparison.OrdinalIgnoreCase))
                {
                    AppendMetadata(value, result);
                    continue;
                }

                if (propName.Equals("product_ids", StringComparison.OrdinalIgnoreCase))
                {
                    AppendPhpArray(propName, value, result);
                    continue;
                }

                if (JsonStringFields.Contains(propName))
                {
                    var jsonValue = JsonConvert.SerializeObject(value);
                    result.Add(new KeyValuePair<string, string>(propName, jsonValue));
                }
                else if (IsComplexType(value))
                {
                    var jsonValue = JsonConvert.SerializeObject(value);
                    result.Add(new KeyValuePair<string, string>(propName, jsonValue));
                }
                else
                {
                    result.Add(new KeyValuePair<string, string>(propName, ConvertToString(value)));
                }
            }
        }

        private static bool IsComplexType(object value)
        {
            if (value == null) return false;
            
            var type = value.GetType();
            
            if (type.IsPrimitive || value is string || value is decimal)
                return false;
            
            if (type.IsEnum)
                return false;
            
            if (value is DateTime)
                return false;
            
            return true;
        }

        private static string ConvertToString(object value)
        {
            if (value is bool b)
                return b ? "1" : "0";

            if (value is DateTime dt)
                return dt.ToString("o");

            return value?.ToString() ?? string.Empty;
        }

        private static void AppendMetadata(object value, List<KeyValuePair<string, string>> result)
        {
            if (!(value is IEnumerable metadataList))
                return;

            int index = 0;

            foreach (var item in metadataList)
            {
                if (!(item is IDictionary<string, object> dict))
                    continue;

                foreach (var kv in dict)
                {
                    result.Add(new KeyValuePair<string, string>(
                        $"metadata[{index}][key]", kv.Key));

                    result.Add(new KeyValuePair<string, string>(
                        $"metadata[{index}][value]", SanitizeMetadataValue(kv.Value)));

                    index++;

                    if (index > 25)
                        throw new InvalidOperationException("Metadata en fazla 25 nesne olabilir.");
                }
            }
        }

        private static string SanitizeMetadataValue(object? value)
        {
            var s = value?.ToString() ?? string.Empty;

            return MetadataValueInvalidChars.Replace(s, "_");
        }

        private static void AppendPhpArray(string fieldName, object value, List<KeyValuePair<string, string>> result)
        {
            if (!(value is IEnumerable list)) return;

            int index = 0;
            foreach (var item in list)
            {
                result.Add(new KeyValuePair<string, string>(
                    $"{fieldName}[{index}]",
                    item?.ToString() ?? string.Empty));
                index++;
            }
        }
    }
}
