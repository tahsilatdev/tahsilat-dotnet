using System;

namespace Tahsilat.NET.Exceptions
{
    /// <summary>
    /// The request did not complete: the API could not be reached, the connection
    /// dropped, the request timed out, or the API answered 424 (its own upstream
    /// failed). When <see cref="Exception.InnerException"/> is set the failure was
    /// local (HttpRequestException, IOException or the timeout cancellation) and
    /// <see cref="TahsilatException.ErrorCode"/> is null.
    ///
    /// A timed out payment, refund or pre-auth may still have been processed by
    /// the server. Query the transaction before sending it again.
    /// </summary>
    public class TahsilatNetworkException : TahsilatException
    {
        public TahsilatNetworkException(string message, string errorCode = null)
            : base(message, errorCode)
        {
        }

        public TahsilatNetworkException(string message, Exception innerException)
            : base(message, null, innerException)
        {
        }
    }
}
