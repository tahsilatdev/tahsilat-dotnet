

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatApiException : TahsilatException
    {
        public int StatusCode { get; }

        public TahsilatApiException(string message, int statusCode, string errorCode = null)
            : base(message, errorCode)
        {
            StatusCode = statusCode;
        }
    }
}
