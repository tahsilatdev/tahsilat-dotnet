

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatAuthenticationException : TahsilatException
    {
        public int StatusCode { get; }

        public TahsilatAuthenticationException(string message, string errorCode = null, int statusCode = 401)
            : base(message, errorCode)
        {
            StatusCode = statusCode;
        }
    }
}
