

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatNotFoundException : TahsilatException
    {
        public TahsilatNotFoundException(string message, string errorCode = null)
            : base(message, errorCode)
        {
        }
    }
}
