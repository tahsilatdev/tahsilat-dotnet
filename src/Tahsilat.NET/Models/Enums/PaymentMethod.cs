namespace Tahsilat.NET.Models.Enums
{
    /// <summary>
    /// Payment method constants matching the Tahsilat API.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>3D Secure payment.</summary>
        ThreeD = 1,

        /// <summary>2D (non-3DS) payment.</summary>
        TwoD = 2
    }
}
