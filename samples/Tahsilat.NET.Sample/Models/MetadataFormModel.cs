namespace Tahsilat.NET.Sample.Models;

/// <summary>
/// "Ürün Metadata Güncelleme" formu. Liste ürünün mevcut metadata'sının
/// TAMAMININ yerine geçer.
/// </summary>
public class MetadataFormModel
{
    public long ProductId { get; set; }

    /// <summary>Satır başına "key=value". Boş bırakılırsa tüm metadata silinir.</summary>
    public string? Metadata { get; set; }
}
