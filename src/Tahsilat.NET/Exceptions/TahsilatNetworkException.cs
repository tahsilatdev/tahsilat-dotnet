

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatNetworkException : TahsilatException
    {
        public TahsilatNetworkException(string message, string errorCode = null)
            : base(message, errorCode)
        {
        }
    }
}
