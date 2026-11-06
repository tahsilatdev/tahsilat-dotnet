using Newtonsoft.Json;
using System;
using System.Net;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Models.Common;

namespace Tahsilat.NET.Infrastructure.Http
{
    /// <summary>
    /// Turns an API error answer into the matching TahsilatException. Shared by
    /// the API calls and the token request so a rate-limited or broken token
    /// endpoint surfaces as the same exception types as any other endpoint.
    /// </summary>
    internal static class ApiErrors
    {
        public static ApiErrorResponse? Parse(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;

            try { return JsonConvert.DeserializeObject<ApiErrorResponse>(body!); }
            catch { return null; }
        }

        public static string FormatMessage(ApiErrorResponse? err, HttpStatusCode code)
        {
            var message = err?.Message ?? $"Tahsilat API error ({(int)code})";

            var errorList = err?.GetErrorList();
            if (errorList != null && errorList.Count > 0)
                message = $"{message} [{string.Join("; ", errorList)}]";

            return message;
        }

        /// <summary>
        /// Builds the exception for a non-2xx answer. Returned rather than thrown
        /// so callers keep their own stack frame and can add context.
        /// </summary>
        public static TahsilatException ForStatus(RawResponse raw, ApiErrorResponse? err)
        {
            var code = raw.StatusCode;
            var errCode = err?.ErrorCode;
            var message = FormatMessage(err, code);

            switch (code)
            {
                case HttpStatusCode.BadRequest:
                case (HttpStatusCode)422:
                    return new TahsilatValidationException(message, errCode, err?.GetErrorDictionary());

                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    return new TahsilatAuthenticationException(message, errCode, (int)code);

                case (HttpStatusCode)402:
                    return new TahsilatPaymentException(message, errCode);

                case HttpStatusCode.NotFound:
                    return new TahsilatNotFoundException(message, errCode);

                case HttpStatusCode.Conflict:
                    return new TahsilatConflictException(message, errCode);

                case (HttpStatusCode)429:
                    var retry = (int)(raw.RetryAfter?.TotalSeconds ?? 0);
                    return new TahsilatRateLimitException(message, retry, errCode);

                case (HttpStatusCode)424:
                    return new TahsilatNetworkException(message, errCode);

                default:
                    return new TahsilatApiException(message, (int)code, errCode);
            }
        }
    }
}
