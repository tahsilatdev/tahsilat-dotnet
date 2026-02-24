

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatRateLimitException : TahsilatException
    {
        public int RetryAfterSeconds { get; }

        public TahsilatRateLimitException(string message, int retryAfter, string errorCode = null)
            : base(message, errorCode)
        {
            RetryAfterSeconds = retryAfter;
        }
    }
}
