

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatValidationException : TahsilatException
    {
        public TahsilatValidationException(string message, string errorCode = null)
            : base(message, errorCode)
        {
        }
    }
}
