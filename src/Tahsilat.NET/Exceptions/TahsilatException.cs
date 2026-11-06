using System;

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatException : Exception
    {
        /// <summary>
        /// The API's error_code when the error came from the API; null otherwise.
        /// </summary>
        public string ErrorCode { get; }

        public TahsilatException(string message, string errorCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public TahsilatException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
