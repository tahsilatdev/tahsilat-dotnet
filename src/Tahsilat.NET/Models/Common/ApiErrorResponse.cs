using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Common
{
    internal class ApiErrorResponse
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("errors")]
        public object Errors { get; set; }

        /// <summary>
        /// Returns errors in a normalized format (handles both array and object formats).
        /// </summary>
        public List<string> GetErrorList()
        {
            if (Errors == null)
                return new List<string>();

            // Already a List<string>
            if (Errors is Newtonsoft.Json.Linq.JArray jArray)
            {
                return jArray.ToObject<List<string>>() ?? new List<string>();
            }

            // Object ise (validation errors)
            if (Errors is Newtonsoft.Json.Linq.JObject jObject)
            {
                var errorList = new List<string>();
                foreach (var prop in jObject.Properties())
                {
                    var fieldName = prop.Name;
                    var fieldErrors = prop.Value.ToObject<List<string>>();
                    if (fieldErrors != null)
                    {
                        foreach (var err in fieldErrors)
                        {
                            errorList.Add($"{fieldName}: {err}");
                        }
                    }
                }
                return errorList;
            }

            return new List<string>();
        }
    }
}
