using Newtonsoft.Json;

namespace Tahsilat.NET.Models.Common
{
    /// <summary>
    /// The <c>edit_lock</c> block of a product (GET products/{id} only). When
    /// <see cref="Locked"/> is true the product is used by an active or pending
    /// subscription: its name, price and billing fields cannot be changed and it
    /// cannot be deleted; the API answers such requests with 409
    /// (<see cref="Exceptions.TahsilatConflictException"/>). <see cref="Reason"/>
    /// carries the API's explanation in Turkish.
    /// </summary>
    public class EditLockInfo
    {
        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
