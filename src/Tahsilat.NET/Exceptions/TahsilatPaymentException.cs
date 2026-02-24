

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatPaymentException : TahsilatException
    {
        public TahsilatPaymentException(string message, string errorCode = null)
            : base(message, errorCode)
        {
        }
    }
}
