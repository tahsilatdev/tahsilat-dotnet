namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// Known values for <see cref="Responses.CommissionResponse.CardType"/>.
    /// The API may introduce new card types, so compare against these constants
    /// instead of switching exhaustively on them.
    /// </summary>
    public static class CardTypes
    {
        public const string Credit = "credit";

        public const string Debit = "debit";

        public const string Prepaid = "prepaid";
    }
}
