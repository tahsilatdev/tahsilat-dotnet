using System;

namespace Tahsilat.NET.Exceptions
{
    public class TahsilatException : Exception
    {
        public string ErrorCode { get; }

        public TahsilatException(string message, string errorCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
