namespace Tahsilat.NET.Sample.Models;

/// <summary>
/// "Ödeme Başlat" formunun alanları. PaymentCreateRequest'e çevrilir.
/// </summary>
public class PaymentFormModel
{
    /// <summary>Kuruş cinsinden toplam tutar; ürünlerin toplamıyla eşleşmeli.</summary>
    public int Amount { get; set; } = 49900;

    public string Currency { get; set; } = "TRY";

    public long? CustomerId { get; set; }

    /// <summary>Virgülle ayrılmış kayıtlı ürün ID'leri. Abonelik yalnız buradan başlar.</summary>
    public string? ProductIds { get; set; }

    /// <summary>Birden fazla abonelik destekli POS varsa zorunlu.</summary>
    public long? SubscriptionCredentialId { get; set; }

    public bool PreAuth { get; set; }

    public string? Description { get; set; }

    /// <summary>Satır başına "key=value".</summary>
    public string? Metadata { get; set; }
}
