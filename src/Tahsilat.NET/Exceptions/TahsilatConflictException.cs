namespace Tahsilat.NET.Exceptions
{
    /// <summary>
    /// The API answered 409: the request clashes with the record's current state.
    /// Examples: deleting a product that is used by an active subscription, or
    /// changing the price or billing fields of such a product.
    /// </summary>
    public class TahsilatConflictException : TahsilatException
    {
        public TahsilatConflictException(string message, string errorCode = null)
            : base(message, errorCode)
        {
        }
    }
}
