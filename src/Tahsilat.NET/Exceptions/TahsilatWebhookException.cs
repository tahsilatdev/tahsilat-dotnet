

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatWebhookException : TahsilatException
    {
        public TahsilatWebhookException(string message, string errorCode = null)
            : base(message, errorCode)
        {
        }
    }
}
