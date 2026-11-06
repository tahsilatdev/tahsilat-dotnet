using System.Collections.Generic;

namespace Tahsilat.NET.Exceptions
{
    /// <summary>
    /// The API rejected the request with 400 or 422. Field errors are listed in
    /// <see cref="Errors"/> and also appended to <see cref="System.Exception.Message"/>.
    /// Note that a missing record can also arrive this way: the API answers some
    /// lookups of a non-existent ID with 422 and error_code 901 rather than 404.
    /// </summary>
    public class TahsilatValidationException : TahsilatException
    {
        private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

        /// <summary>
        /// Validation messages keyed by field name, e.g. "amount" => ["The amount
        /// field is required."]. Messages the API did not tie to a field are under
        /// the key "*". Empty when the API sent no errors block.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public TahsilatValidationException(string message, string errorCode = null)
            : this(message, errorCode, null)
        {
        }

        public TahsilatValidationException(string message, string errorCode, IReadOnlyDictionary<string, string[]> errors)
            : base(message, errorCode)
        {
            Errors = errors ?? NoErrors;
        }
    }
}
