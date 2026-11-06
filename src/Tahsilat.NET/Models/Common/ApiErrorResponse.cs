using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// The envelope every API answer shares, read before the typed body so that
    /// HTTP errors and <c>"status": false</c> answers can be turned into exceptions.
    /// </summary>
    internal class ApiErrorResponse
    {
        /// <summary>
        /// Null when the body carried no status field at all.
        /// </summary>
        [JsonProperty("status")]
        public bool? Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Either an object keyed by field name ({"amount": ["..."]}) or a plain
        /// array of messages.
        /// </summary>
        [JsonProperty("errors")]
        public JToken Errors { get; set; }

        /// <summary>
        /// Key used in <see cref="GetErrorDictionary"/> for messages the API did not
        /// tie to a field.
        /// </summary>
        public const string GeneralErrorKey = "*";

        /// <summary>
        /// Returns errors as "field: message" lines (handles both array and object formats).
        /// </summary>
        public List<string> GetErrorList()
        {
            var result = new List<string>();

            foreach (var pair in GetErrorDictionary())
            {
                foreach (var message in pair.Value)
                {
                    result.Add(pair.Key == GeneralErrorKey ? message : $"{pair.Key}: {message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Returns errors keyed by field name; array-form errors go under
        /// <see cref="GeneralErrorKey"/>. Empty when there is no errors block.
        /// </summary>
        public Dictionary<string, string[]> GetErrorDictionary()
        {
            var result = new Dictionary<string, string[]>();

            if (Errors is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    var messages = ToMessages(prop.Value);
                    if (messages.Length > 0)
                        result[prop.Name] = messages;
                }
            }
            else if (Errors is JArray arr)
            {
                var messages = ToMessages(arr);
                if (messages.Length > 0)
                    result[GeneralErrorKey] = messages;
            }

            return result;
        }

        private static string[] ToMessages(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return new string[0];

            if (token is JArray arr)
            {
                var list = new List<string>(arr.Count);
                foreach (var item in arr)
                {
                    var text = item.Type == JTokenType.String ? item.Value<string>() : item.ToString(Formatting.None);
                    if (!string.IsNullOrEmpty(text))
                        list.Add(text);
                }
                return list.ToArray();
            }

            var single = token.Type == JTokenType.String ? token.Value<string>() : token.ToString(Formatting.None);
            return string.IsNullOrEmpty(single) ? new string[0] : new[] { single };
        }
    }
}
