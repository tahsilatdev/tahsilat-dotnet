# Tahsilat .NET Library

Tahsilat Payment Gateway için resmi .NET Library.

[![NuGet](https://img.shields.io/nuget/v/tahsilat-dotnet.svg)](https://www.nuget.org/packages/tahsilat-dotnet)

## Gereksinimler

| Platform | Versiyon |
|----------|----------|
| .NET Framework | 4.5.2, 4.6.2, 4.7.2, 4.8 |
| .NET Standard | 2.0, 2.1 |
| .NET | 8.0, 9.0 |

> **Not:** `.NET Standard 2.0/2.1` desteği sayesinde .NET 5, 6, 7, 10 ve sonraki tüm .NET sürümleri uyumludur.

## Kurulum

### NuGet ile

```bash
dotnet add package tahsilat-dotnet
```

### Package Manager Console

```powershell
Install-Package tahsilat-dotnet
```

## Hızlı Başlangıç

### Client Başlatma
```csharp
//Doğrudan bir şekilde Controller ya da servis içinde kullanabilirsiniz.

using Tahsilat.NET;

// Sandbox (test) ortamı
var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");

// Production (canlı) ortamı
var tahsilat = new TahsilatClient("sk_live_YOUR_SECRET_KEY");
```

### Dependency Injection (.NET 6+)

```csharp
// Program.cs
using Tahsilat.NET.Extensions;

builder.Services.AddTahsilat(options =>
{
    options.ApiKey = "sk_test_YOUR_SECRET_KEY";
    options.TimeoutSeconds = 80;      // Varsayılan: 80 saniye (banka yanıtları yavaş olabilir)
    options.MaxNetworkRetries = 3;    // Varsayılan: 3 — bkz. "Timeout ve Bağlantı Hataları"
    // options.HttpMessageHandler = new HttpClientHandler { Proxy = ... }; // proxy / loglama / test
});
```

```csharp
// Controller veya Service içinde
public class PaymentController : Controller
{
    private readonly ITahsilatClient _tahsilat;

    public PaymentController(ITahsilatClient tahsilat)
    {
        _tahsilat = tahsilat;
    }
}
```

> **Önemli:** Sadece secret key'ler (`sk_test_*` veya `sk_live_*`) kabul edilir. Public key'ler (`pk_*`) server-side API çağrıları için kullanılamaz.

## Kullanım Örnekleri

### Müşteri Oluşturma
```csharp
var request = new CustomerCreateRequest
{
    Name = "Test",
    LastName = "User",
    Email = "testuser@mail.com",
    Phone = "+901234567890",
    Country = "TR",
    City = "İstanbul",
    District = "Sarıyer",
    Address = "Sarıyer, İstanbul",
    ZipCode = "34000",
    Metadata = new()
    {
        new Dictionary<string, object>
        {
            ["customer_name"] = "testuser",
            ["customer_type"] = "premium"
        },
        new Dictionary<string, object>
        {
            ["customer_created"] = "Today",
            ["source"] = "tahsilat-dotnet"
        }
    }
};

var response = await tahsilat.Customers.CreateAsync(request);
```

> **Metadata kuralları:** en fazla 25 kayıt; `key` küçük harfle başlar, yalnızca `a-z`, `0-9`, `_` içerir ve en fazla 20 karakterdir; `value` en fazla 200 karakterdir ve **olduğu gibi gönderilir** (`"İstanbul şubesi"` gibi Türkçe karakterler korunur).

Müşteri servisinin diğer metotları:

```csharp
var customer = await tahsilat.Customers.GetAsync(20585467989184);
var updated  = await tahsilat.Customers.UpdateAsync(20585467989184, new CustomerUpdateRequest { City = "Ankara" });
var deleted  = await tahsilat.Customers.DeleteAsync(20585467989184); // bool döner
```

#### Müşteri Arama

Arama bir filtre nesnesi alır, **sayfalı** bir sonuç döner. Hiç filtre vermezseniz ilk sayfa gelir:

```csharp
var page = await tahsilat.Customers.SearchAsync(new CustomerSearchRequest
{
    Search = "ayşe",                  // ad, e-posta ve telefonda serbest arama
    Email = "ayse@mail.com",
    CustomerDate = new DateRangeFilter
    {
        StartDate = new DateTime(2026, 1, 1),
        EndDate   = new DateTime(2026, 1, 31)
    },
    Metadata = new MetadataFilter { Key = "customer_type", Value = "premium" },
    Page = 1,                         // ya da Start = 0 (satır ofseti)
    Limit = 50                        // 1-100
});

Console.WriteLine(page.Meta.Total);   // toplam eşleşen kayıt
foreach (var c in page.Customers)
    Console.WriteLine($"{c.Id} {c.NameLastName} {c.Email}");
```

Kullanılabilir filtreler: `Search`, `CustomerId`, `NameLastName`, `Email`, `Phone`, `CustomerDate` (tarih aralığı), `Metadata` (key/value). Filtreler AND ile birleşir.

### Ürün Oluşturma
```csharp
var request = new ProductCreateRequest
{
    ProductName = "Test Product",
    Price = 75900, // Kuruş cinsinden: 759,00 TL
    Description = "Integration Test Product",
    Metadata = new()
    {
        new Dictionary<string, object>
        {
            ["product_name"] = "Test Product",
            ["product_type"] = "phone"
        },
        new Dictionary<string, object>
        {
            ["product_created"] = "Today",
            ["source"] = "tahsilat-dotnet"
        }
    }
};

var response = await tahsilat.Products.CreateAsync(request);
```

Ürün servisinin diğer metotları:

```csharp
var product = await tahsilat.Products.GetAsync(55437751141488);
var updated = await tahsilat.Products.UpdateAsync(55437751141488, new ProductUpdateRequest { Price = 89900 });
var deleted = await tahsilat.Products.DeleteAsync(55437751141488); // bool döner
```

#### Ürün Arama

```csharp
var page = await tahsilat.Products.SearchAsync(new ProductSearchRequest
{
    ProductName = "Premium",
    Financial = new PriceFilter { Price = 10000, Operator = ">=" },   // kuruş
    ProductDate = new DateRangeFilter { StartDate = new DateTime(2026, 1, 1) },
    Metadata = new MetadataFilter { Key = "product_type" },
    Page = 1,
    Limit = 50
});

Console.WriteLine(page.Meta.Total);
foreach (var p in page.Products)
    Console.WriteLine($"{p.Id} {p.ProductName} {p.FormattedPrice}");
```

Kullanılabilir filtreler: `ProductId`, `ProductName`, `Financial` (fiyat + operatör), `ProductDate`, `Metadata`.

#### Ürün Metadata'sını Güncelleme

`UpdateMetadataAsync` gönderdiğiniz listeyi ürünün **mevcut metadata'sının tamamının yerine** koyar; listede olmayan kayıtlar silinir. Korumak istediklerinizi yeniden gönderin:

```csharp
var product = await tahsilat.Products.UpdateMetadataAsync(55437751141488, new()
{
    new Dictionary<string, object> { ["product_type"] = "phone" },
    new Dictionary<string, object> { ["source"] = "tahsilat-dotnet" }
});
```

### Abonelik (Tekrarlayan Ödeme)

Abonelik, ürün üzerinde tanımlanır; ayrı bir endpoint yoktur. Önce abonelik ürünü oluşturun, sonra bu ürünle `ProductIds` üzerinden normal bir ödeme başlatın. Müşteri ilk ödemeyi 3D Secure ile tamamladığında abonelik otomatik oluşur.

```csharp
using Tahsilat.NET.Models.Enums;

// 1. Abonelik ürünü
var product = await tahsilat.Products.CreateAsync(new ProductCreateRequest
{
    ProductName = "Premium Üyelik",
    Price = 49900,
    IsRecurring = true,
    BillingInterval = BillingInterval.Month, // Day, Week, Month, Year
    IntervalCount = 1                        // 1-366: Month + 2 = iki ayda bir
});

Console.WriteLine(product.RecurringText);    // "Aylık" — ödeme sayfasında ve e-postalarda görünür

// 2. İlk ödeme
var payment = await tahsilat.Payments.CreateAsync(new PaymentCreateRequest
{
    Amount = 49900,
    Currency = "TRY",
    CustomerId = 50012535976895,
    ProductIds = new List<long> { product.Id },
    // Birden fazla abonelik destekli POS'unuz varsa zorunlu, aksi halde 422 döner:
    // SubscriptionCredentialId = 123,
});
```

Kurallar:

- Abonelik yalnız `ProductIds` ile başlar; anlık `Products` listesiyle başlamaz.
- Aynı ödemedeki abonelik ürünleri aynı dönemde olmalıdır (`4003`); taksit kullanılamaz (`4005`); "Sadece Taksit" ile birlikte kullanılamaz (`4004`).
- Özellik hesabınızda etkin değilse istekler `4002` ya da `4007` ile reddedilir.
- Aktif ya da ilk ödemesi bekleyen bir aboneliğe bağlı üründe ad, fiyat ve abonelik alanları değiştirilemez, ürün silinemez; API `409` döner (`TahsilatConflictException`).

Sonraki dönem tahsilatları **yeni bir `transaction_id` ile ayrı işlem** olarak gelir. İşlem sonucu ve webhook yükündeki `Subscription` bloğu abonelik kimliğini ve dönem numarasını taşır; abonelik dışı işlemlerde `null`dır:

```csharp
using Tahsilat.NET.Extensions;

var transaction = await tahsilat.Transactions.RetrieveAsync(75690998341504);

if (transaction.HasSubscription())
{
    var subscriptionId = transaction.GetSubscriptionId();
    var cycleNumber    = transaction.GetSubscriptionCycleNumber();   // 0 = ilk 3D ödeme
    var isFirst        = transaction.IsInitialSubscriptionPayment();
    var statusText     = transaction.GetSubscriptionStatusTextTr();  // "Aktif", "Ödeme Bekliyor", ...

    // Ham alanlar: transaction.Subscription.Status (SubscriptionStatus enum değerleri),
    // NextBillingAt, CancelledAt, StatusText
}
```

| `SubscriptionStatus` | Değer | Anlamı |
|---|---|---|
| `Pending` | 5 | İlk ödeme bekleniyor |
| `Active` | 1 | Aktif, sonraki tahsilat `NextBillingAt` tarihinde |
| `PastDue` | 2 | Son tahsilat başarısız, tekrar deneniyor |
| `Cancelled` | 3 | İptal edildi, yeni tahsilat yapılmaz |
| `Completed` | 4 | Planlanan tüm dönemler tahsil edildi |

> `cycle_number` yalnız dönem değişince artar; aynı dönemdeki başarısız denemeler aynı numarayı taşır. Bir dönemi yalnız **başarılı** sonuçta ödenmiş sayın. Webhook'taki durum gönderim anındaki durumdur; güncel durum için işlemi sorgulayın.

### Ödeme Oluşturma

#### Ürün Bilgileri ile
```csharp
var request = new PaymentCreateRequest
{
    Currency = "TRY",
    Amount = 70000,
    RedirectUrl = "https://example.com/payment/callback",
    Products = new List<ProductItem>
    {
        new ProductItem
        {
            ProductName = "Product1",
            Price = 50000,
            Description = "Test Product"
        },
        new ProductItem
        {
            ProductName = "Product2",
            Price = 10000,
            Description = "Test Product"
        },
        new ProductItem
        {
            ProductName = "Product3",
            Price = 10000,
            Description = "Test Product"
        }
    },
    Metadata = new()
    {
        new Dictionary<string, object>
        {
            ["order_id"] = 123456,
            ["customer_type"] = "premium"
        },
        new Dictionary<string, object>
        {
            ["created"] = "Subat2026",
            ["source"] = "Tahsilat-dotnet-test"
        }
    },
    Description = "Integration Test Product"
};

var response = await tahsilat.Payments.CreateAsync(request);
```



#### Kayıtlı Ürün ID'leri ile
```csharp
var request = new PaymentCreateRequest
{
    Amount = 50000,
    Currency = "TRY",
    RedirectUrl = "https://example.com/payment/callback",
    ProductIds = new List<long>
    {
        55437751141488,
        84920468860151
    },
    CustomerId = 20585467989184,
    Metadata = new()
    {
        new Dictionary<string, object>
        {
            ["order_id"] = 123456,
            ["customer_type"] = "premium"
        },
        new Dictionary<string, object>
        {
            ["created"] = "Subat2026",
            ["source"] = "Tahsilat-dotnet-test"
        }
    },
};

var response = await tahsilat.Payments.CreateAsync(request);
```

#### İstek Alanları

| Alan | Zorunlu | Açıklama |
|------|---------|----------|
| `Amount` | ✅ | Kuruş cinsinden toplam tutar (ör. `70000` = 700,00 TL) |
| `Currency` | ✅ | ISO 4217 para birimi kodu (ör. `TRY`) |
| `Products` | ⚠️ | Ürün listesi. `ProductIds` göndermiyorsanız zorunlu |
| `ProductIds` | ⚠️ | Kayıtlı ürün ID'leri. `Products` göndermiyorsanız zorunlu |
| `RedirectUrl` | ❌ | Ödeme sonrası dönülecek adres. **Boş bırakılırsa Tahsilat'ın sonuç sayfası gösterilir** |
| `CustomerId` | ❌ | İşlemi kayıtlı bir müşteriyle ilişkilendirir |
| `PreAuth` | ❌ | `true` ise tutar çekilmez, yalnızca bloke edilir (varsayılan `false`) |
| `Description` | ❌ | İşlem açıklaması |
| `Metadata` | ❌ | Raporlama için ek veri (en fazla 25 nesne) |
| `SubscriptionCredentialId` | ⚠️ | Abonelik ürünü içeren ödemede tahsilatın yapılacağı POS. Birden fazla abonelik destekli POS varsa zorunlu |
| `WhiteLabel` | ❌ | Yalnız white label yetkisi olan firmalar: kart bilgisi kendi sayfanızda alınır |
| `InstallmentCount` | ❌ | Taksit sayısı; yalnız `WhiteLabel = true` iken dikkate alınır. Abonelik ürünüyle kullanılamaz |

#### Ödeme Yanıtının Kullanımı

`CreateAsync` bir `PaymentResponse` döner. Müşteriyi `PaymentPageUrl` adresine yönlendirmeniz gerekir:

```csharp
var response = await tahsilat.Payments.CreateAsync(request);

Console.WriteLine(response.TransactionId);   // İşlem numarası — kendi tarafınızda saklayın
Console.WriteLine(response.PaymentPageUrl);  // Müşterinin yönlendirileceği ödeme sayfası
Console.WriteLine(response.ExpiresAt);       // Ödeme sayfasının geçerlilik süresi

// ASP.NET Core örneği
return Redirect(response.PaymentPageUrl);
```

#### Ödeme Sonrası Yönlendirme (`RedirectUrl`)

`RedirectUrl` **opsiyoneldir** ve davranışı gönderilip gönderilmemesine göre değişir:

| Durum | Ödeme sonrası ne olur |
|-------|-----------------------|
| `RedirectUrl` **verilir** | Müşteri sizin belirttiğiniz adrese döner |
| `RedirectUrl` **boş bırakılır / gönderilmez** | Müşteri **Tahsilat'ın kendi ödeme sonuç sayfasına** yönlendirilir |

```csharp
// Müşteri kendi sitenize döner
var request = new PaymentCreateRequest
{
    Amount = 70000,
    Currency = "TRY",
    RedirectUrl = "https://example.com/payment/callback"
};

// RedirectUrl verilmezse müşteri Tahsilat'ın sonuç sayfasında kalır
var request = new PaymentCreateRequest
{
    Amount = 70000,
    Currency = "TRY"
};
```

> **Dikkat:** Müşteriyi ödeme sonrasında kendi sitenize geri almak istiyorsanız `RedirectUrl` göndermek **zorundasınız**. Boş bırakırsanız akış Tahsilat'ta biter ve müşteri sitenize dönmez.

> `RedirectUrl` adresi query parametresi olarak yalnızca `transaction_id` içermelidir. `null` bıraktığınızda alan istek gövdesine hiç yazılmaz.

#### Ön Provizyon (Pre-Auth)

`PreAuth = true` gönderirseniz tutar karttan çekilmez, yalnızca bloke edilir:

```csharp
var request = new PaymentCreateRequest
{
    Amount = 50000,
    Currency = "TRY",
    RedirectUrl = "https://example.com/payment/callback",
    PreAuth = true
};

var response = await tahsilat.Payments.CreateAsync(request);
```

Bloke edilen tutarı sonradan **kapatmanız (capture)** veya **iptal etmeniz (void)** gerekir:

```csharp
// Provizyonu kapat — tutar tahsil edilir
var approve = await tahsilat.Transactions.ResolvePreAuthAsync(new PreAuthResolveRequest
{
    TransactionId = 78810412652494,
    Status = true
});

// Provizyonu iptal et — bloke çözülür, tahsilat yapılmaz
var cancel = await tahsilat.Transactions.ResolvePreAuthAsync(new PreAuthResolveRequest
{
    TransactionId = 78810412652494,
    Status = false
});

if (approve.Status)
{
    Console.WriteLine(approve.Message);
}
```

> `ResolvePreAuthAsync` ve senkron `ResolvePreAuth` bir `ApiResponse<PreAuthResolveResponse>` döner. Banka kapatmayı reddederse yanıt `Status = false` ile gelmez, `TahsilatApiException` fırlar (bkz. "Hata Yönetimi").

### İşlem Sorgulama
```csharp
var transaction = await tahsilat.Transactions.RetrieveAsync(78810412652494);

Console.WriteLine(transaction.TransactionId);
Console.WriteLine(transaction.PaymentStatusText); // success, fail, incomplete
Console.WriteLine(transaction.TransactionStatusText); // completed, pending, cancelled
Console.WriteLine(transaction.Amount);

// Başarı kontrolü
if (transaction.PaymentStatus == 1) { //Success
    Console.WriteLine("Ödeme Başarılı");
}

if (transaction.PaymentStatus == 2) {
    Console.WriteLine("Ödeme Başarısız.");
}

if (transaction.PaymentStatus == 3) {
    Console.WriteLine("Ödeme henüz tamamlanmadı.");
}

// Aynı kontroller extension metotlarıyla:
using Tahsilat.NET.Extensions;
if (transaction.IsSuccess()) { /* ... */ }
if (transaction.HasSubscription()) { /* bkz. "Abonelik (Tekrarlayan Ödeme)" */ }
```

> Var olmayan bir işlem sorgulandığında API `404` yerine `422` + `error_code: 901` dönebilir; bu durumda `TahsilatNotFoundException` değil `TahsilatValidationException` fırlar. İkisini birlikte yakalayın.

### İade İşlemi

İade işlemlerinin tek giriş noktası `tahsilat.Transactions.RefundAsync` metodudur.

#### Tam İade

`Amount` alanı **opsiyoneldir**. Boş (null) bırakırsanız işlem tutarının tamamı iade edilir:

```csharp
var request = new RefundCreateRequest
{
    TransactionId = 78810412652494,
    Description = "Müşteri talebi ile iade"
};

var response = await tahsilat.Transactions.RefundAsync(request);
```

#### Kısmi İade

`Amount` alanına değer verirseniz kısmi iade yapılır. Tutar **kuruş cinsindendir**, en az `100` (1,00 TL) olmalı ve işlem tutarını aşmamalıdır:

```csharp
var request = new RefundCreateRequest
{
    TransactionId = 78810412652494,
    Amount = 1000, // Kısmi iade (10.00 TL)
    Description = "Müşteri talebi ile iade"
};

var response = await tahsilat.Transactions.RefundAsync(request);
```

| Alan | Zorunlu | Açıklama |
|------|---------|----------|
| `TransactionId` | ✅ | İade edilecek işlemin ID'si |
| `Amount` | ❌ | Kuruş cinsinden iade tutarı (min `100`). **Boş bırakılırsa tam iade yapılır.** |
| `Description` | ✅ | İade açıklaması (en fazla 255 karakter) |

#### İade Yanıtının Okunması

`RefundAsync`, diğer metotlardan farklı olarak **`ApiResponse<RefundResponse>`** döner. Başarılı sonucu `Message` alanından okursunuz; banka iadeyi reddederse **`TahsilatApiException` fırlar**:

```csharp
try
{
    var response = await tahsilat.Transactions.RefundAsync(request);

    // "İade işlemi başarıyla gerçekleştirildi ve tutar bakiyenizden düşüldü."
    Console.WriteLine(response.Message);
}
catch (TahsilatApiException ex) when (ex.StatusCode == 200)
{
    // Banka reddetti — iade beklemede kalır, tekrar denenebilir
    Console.WriteLine($"İade reddedildi: {ex.Message} (kod: {ex.ErrorCode})");
}
```

> **Önemli:** İade endpoint'i **`Data` alanını doldurmaz, her zaman `null` döner.** İade kaydının detaylarına (tutar, banka referans kodu, durum) bu yanıttan erişemezsiniz. İşlemin güncel durumunu görmek için `Transactions.RetrieveAsync(transactionId)` ile işlemi yeniden sorgulayın ya da webhook'u dinleyin.

> **1.2.0 davranış değişikliği:** Önceki sürümlerde banka reddi `Status = false` ile sessizce dönüyordu. Artık API `200` içinde `"status": false` gönderdiğinde SDK, tahsilat-php ile aynı şekilde, `TahsilatApiException` fırlatır (`StatusCode = 200`, `ErrorCode` API'nin hata kodu). `Status` kontrolü yapan eski kodu `try/catch` ile değiştirin.

> **Not:** Bir işlem üzerinde önceki iade tamamlanmadan yeni iade başlatılamaz.

> **Not:** Senkron kullanım için `tahsilat.Transactions.Refund(request)` metodunu kullanabilirsiniz.

### BIN Sorgulama

```csharp
var response = await tahsilat.BinLookup.DetailAsync(48945540);

Console.WriteLine(response.BankName);
Console.WriteLine(response.CardType);
Console.WriteLine(response.CardBrand);
```


### Komisyon Sorgulama
```csharp
var commissions = await tahsilat.Commissions.SearchAsync();

// BIN numarasına göre filtrele
var filtered = await tahsilat.Commissions.SearchAsync(new CommissionSearchRequest 
{ 
    BinNumber = 48945540 
});

```

#### Yanıt Satırındaki Kart Boyutu Alanları

Aynı komisyon oranı listede birden fazla kez görünebilir. Bunun sebebi, her satırın **farklı bir kart senaryosuna** ait olmasıdır. Bir satır şu kombinasyonla tekilleşir: `Installment` + `CardType` + `IsOnUs` + `IsForeign`.

| Alan | Tip | Anlam |
|------|-----|-------|
| `CompanyPosCredentialId` | `long?` | Oranın ait olduğu POS kredensiyalinin kimliği |
| `PosId` | `long?` | Oranın ait olduğu POS entegrasyonunun kimliği |
| `PosName` | `string` | Oranın ait olduğu POS'un adı (ör. `Ziraat Pay Pos`) |
| `InstallmentText` | `string` | Taksit açıklaması (ör. `Tek çekim`) |
| `CardType` | `string` | Oranın geçerli olduğu kart türü: `credit` / `debit` / `prepaid`. **`null` = tüm kart türleri** |
| `IsOnUs` | `bool?` | `true` = kartı çıkaran bankanın kendi POS'una (on-us) ait oran, `false` = on-us değil, **`null` = her ikisi için geçerli** |
| `IsForeign` | `bool` | `true` = yabancı (yurt dışı) kart oranı, `false` = yerli kart |

> **Dikkat:** `CardType` ve `IsOnUs` alanlarında `null` bir eksiklik değil, **anlamlı bir değerdir**. `CardType == null` "her kart türü için geçerli", `IsOnUs == null` ise "hem on-us hem not-on-us için geçerli" demektir.

`CardType` karşılaştırması için `CardTypes` sabitlerini kullanabilirsiniz:

```csharp
using Tahsilat.NET.Models.Common;

// Yerli kredi kartı, tek çekim oranları
var credit = commissions
    .Where(c => c.Installment == 1)
    .Where(c => !c.IsForeign)
    .Where(c => c.CardType == CardTypes.Credit || c.CardType == null)
    .ToList();

foreach (var c in credit)
{
    Console.WriteLine($"{c.PosName} · {c.InstallmentText} · %{c.CommissionRate}");
}
```

> `CardTypes` sabitleri (`Credit`, `Debit`, `Prepaid`) yalnızca bilinen değerleri içerir. API ileride yeni bir kart türü ekleyebileceği için `CardType` alanını `string` olarak karşılaştırın, tüm olasılıkları kapsayan bir `switch` yazmayın.

#### Çoklu POS Davranışı

`BinNumber` **göndermediğiniz** istekte liste, sadece birincil POS'un değil, üye işyerinin **tüm aktif POS'larının** oranlarını döner. Bu yüzden aynı taksit sayısı birden fazla satırda görünebilir:

```csharp
var all = await tahsilat.Commissions.SearchAsync();

// POS bazında grupla
foreach (var group in all.GroupBy(c => c.PosName))
{
    Console.WriteLine($"--- {group.Key} ---");
    foreach (var c in group.OrderBy(c => c.Installment))
        Console.WriteLine($"{c.InstallmentText}: %{c.CommissionRate}");
}
```

`BinNumber` **gönderdiğiniz** istekte ise her satır, o taksit için kazanan POS'u gösterir.

> Taksit sayısına göre tek bir oran seçen mevcut kodunuz (`commissions.First(c => c.Installment == 3)` gibi) artık rastgele bir POS'un oranını dönebilir. Böyle bir yerde `PosName` / `PosId` ile filtreleme yapın.

## Hata Yönetimi
```csharp

using Tahsilat.NET.Exceptions;

try
{
    var payment = await tahsilat.Payments.CreateAsync(new PaymentCreateRequest
    {
        Amount = 10000,
        Currency = "TRY",
        RedirectUrl = "https://example.com/payment/callback"
    });
}
catch (TahsilatAuthenticationException ex)
{
    // Geçersiz API key ya da yetki hatası (401 / 403).
    // Süresi dolan access token bu bloğa düşmez: SDK yeni token alıp isteği bir kez tekrarlar.
    Console.WriteLine($"Kimlik doğrulama hatası: {ex.Message}");
    Console.WriteLine($"HTTP Durum Kodu: {ex.StatusCode}");
}
catch (TahsilatValidationException ex)
{
    // Geçersiz istek parametreleri (400 / 422)
    Console.WriteLine($"Validasyon hatası: {ex.Message}");
    Console.WriteLine($"Hata Kodu: {ex.ErrorCode}");

    // Alan bazlı hatalar: { "amount": ["Tutar zorunludur."], "currency": [...] }
    // Alana bağlı olmayan mesajlar "*" anahtarı altındadır.
    foreach (var field in ex.Errors)
        Console.WriteLine($"{field.Key}: {string.Join(", ", field.Value)}");
}
catch (TahsilatNotFoundException ex)
{
    // Kaynak bulunamadı (404)
    Console.WriteLine($"Bulunamadı: {ex.Message}");
}
catch (TahsilatConflictException ex)
{
    // Kaydın mevcut durumuyla çelişen istek (409):
    // aktif aboneliği olan ürünü silmek / fiyatını değiştirmek gibi
    Console.WriteLine($"Çakışma: {ex.Message}");
}
catch (TahsilatPaymentException ex)
{
    // Ödeme işlemi hatası
    Console.WriteLine($"Ödeme hatası: {ex.Message}");
    Console.WriteLine($"Hata Kodu: {ex.ErrorCode}");
}
catch (TahsilatRateLimitException ex)
{
    // Rate limit aşıldı (429)
    Console.WriteLine($"Rate limit: {ex.Message}");
    Console.WriteLine($"Tekrar deneme süresi: {ex.RetryAfterSeconds} saniye");
}
catch (TahsilatNetworkException ex)
{
    // İstek tamamlanamadı: bağlantı kurulamadı, bağlantı koptu, timeout
    // (ex.InnerException dolu) ya da API 424 döndü (bağımlılık hatası).
    // Bkz. "Timeout ve Bağlantı Hataları"
    Console.WriteLine($"Ağ hatası: {ex.Message}");
}
catch (TahsilatApiException ex)
{
    // Diğer API hataları (5xx vb.) ve 200 içinde "status": false dönen yanıtlar
    // (örn. bankanın reddettiği iade — StatusCode 200, ErrorCode API'nin kodu)
    Console.WriteLine($"API Hatası: {ex.Message}");
    Console.WriteLine($"HTTP Durum Kodu: {ex.StatusCode}");
    Console.WriteLine($"Hata Kodu: {ex.ErrorCode}");
}
catch (TahsilatException ex)
{
    // Genel SDK hatası (tüm Tahsilat exception'larının base sınıfı)
    Console.WriteLine($"Hata: {ex.Message}");
    Console.WriteLine($"Hata Kodu: {ex.ErrorCode}");
}
```

### Exception Hiyerarşisi

| Exception | Açıklama | Özel Property'ler |
|-----------|----------|-------------------|
| `TahsilatException` | Tüm SDK hatalarının base sınıfı | `ErrorCode` |
| ├─ `TahsilatAuthenticationException` | Geçersiz API key / yetki hatası (401, 403) | `StatusCode` |
| ├─ `TahsilatValidationException` | Geçersiz istek parametreleri (400, 422) | `Errors` (alan → mesajlar) |
| ├─ `TahsilatNotFoundException` | Kaynak bulunamadı (404) | — |
| ├─ `TahsilatConflictException` | Kaydın durumuyla çelişen istek (409) | — |
| ├─ `TahsilatPaymentException` | Ödeme işlemi hatası (402) | — |
| ├─ `TahsilatRateLimitException` | İstek limiti aşıldı (429) | `RetryAfterSeconds` |
| ├─ `TahsilatNetworkException` | Bağlantı/timeout hatası ya da API'den 424 | `InnerException` (yerel hatalarda) |
| ├─ `TahsilatApiException` | Diğer API hataları ve `200` + `"status": false` | `StatusCode` |
| └─ `TahsilatWebhookException` | Webhook doğrulama hatası | — |

> **Not:** İade endpoint'i pratikte `200, 400, 403, 404, 422, 429, 500` durum kodlarını döndürür; `402` ve `424` döndürmez. SDK'daki `TahsilatPaymentException` (402) ve `TahsilatNetworkException` (424) eşlemeleri savunma amaçlı genel eşlemelerdir ve diğer endpoint'ler için korunmaktadır.

### Timeout ve Bağlantı Hataları

Yanıtın hiç dönmediği durumlar (`DNS çözülemedi`, bağlantı reddedildi, SSL hatası, bağlantı koptu, `TimeoutSeconds` aşıldı) **`TahsilatNetworkException`** olarak fırlar; ham `HttpRequestException` / `TaskCanceledException` `InnerException` içinde durur. Yani `catch (TahsilatException)` bu durumları da yakalar. (Sizin verdiğiniz `CancellationToken` iptal edildiğinde ise `OperationCanceledException` olduğu gibi fırlar.)

SDK, transport hatalarında **güvenli olduğu ölçüde** otomatik tekrar dener (`MaxNetworkRetries`, varsayılan 3; `0` kapatır):

| Hata | Tekrar denenir mi? | Neden |
|------|-------------------|-------|
| Bağlantı kurulamadı (DNS, TCP, TLS) | ✅ Her istekte | İstek sunucuya hiç ulaşmadı |
| Timeout ya da bağlantı koptu — **GET** ve token isteği | ✅ | Tekrarın yan etkisi yok |
| Timeout ya da bağlantı koptu — **POST** (ödeme, iade, ön provizyon) | ❌ | İstek sunucuda işlenmiş olabilir; tekrar etmek **ikinci bir iade / ödeme** oluşturabilir |

Denemeler arasında 0,5 s, 1 s, 1,5 s... (en fazla 2 s) beklenir.

> **Uyarı:** Timeout alan bir mutasyon çağrısını kendiniz de körlemesine tekrarlamayın. Önce işlemin güncel durumunu sorgulayın:

```csharp
using Tahsilat.NET.Extensions;

try
{
    var response = await tahsilat.Transactions.RefundAsync(request);
}
catch (TahsilatNetworkException ex) when (ex.InnerException != null)
{
    // Bağlantı/timeout: iade sunucuda gerçekleşmiş OLABİLİR — doğrulamadan tekrar etme
    var transaction = await tahsilat.Transactions.RetrieveAsync(request.TransactionId);

    if (transaction.HasRefund())
        return; // İade zaten işlenmiş, tekrar gönderme

    // İade işlenmemiş, güvenle tekrar denenebilir
}
```

`TimeoutSeconds` varsayılan olarak **80 saniyedir** (banka yanıtları yavaş olabilir) ve `TahsilatClientOptions` üzerinden değiştirilebilir.


## API Key Türleri

| Key Türü    | Format      | Kullanım |
|-------------|-------------|----------|
| Secret Test | `sk_test_*` | Test ortamı - tam erişim |
| Secret Live | `sk_live_*` | Canlı ortam - tam erişim |    

> **Not:** Public key'ler (`pk_test_*`, `pk_live_*`) bu SDK ile kullanılamaz. Client-side işlemler için JavaScript SDK kullanın.

## Webhook Doğrulama

> **Uyarı:** Webhook endpoint'iniz harici bir POST isteği aldığı için CSRF korumasından muaf tutulmalıdır.

Her webhook isteği `X-Tahsilat-Signature` başlığı ile HMAC-SHA256 imzası içerir. İmza formatı: `t=timestamp,v1=signature`.

```csharp
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Webhooks;

[HttpPost("webhook")]
public async Task<IActionResult> Webhook()
{
    // 1. Request body'yi oku
    using var ms = new MemoryStream();
    await Request.Body.CopyToAsync(ms);
    var payloadBytes = ms.ToArray();

    // 2. Signature header'ını al
    var signature = Request.Headers["X-Tahsilat-Signature"].FirstOrDefault() ?? string.Empty;

    try
    {
        // 3. Webhook event'i doğrula ve parse et.
        //    Replay koruması varsayılan olarak açıktır: 5 dakikadan eski (ya da
        //    5 dakikadan ileri tarihli) webhook'lar reddedilir.
        var webhookEvent = WebhookHandler.ConstructEvent(payloadBytes, signature, "whsec_YOUR_WEBHOOK_SECRET");

        // 4. Ödeme durumuna göre işlem yap
        if (webhookEvent.IsSuccess())
        {
            // Ödeme başarılı
            Console.WriteLine($"Ödeme başarılı! Transaction ID: {webhookEvent.TransactionId}");
            Console.WriteLine($"Tutar: {webhookEvent.FormattedAmount} {webhookEvent.CurrencyCode}");
        }
        else if (webhookEvent.IsFailed())
        {
            // Ödeme başarısız
            Console.WriteLine($"Ödeme başarısız. Transaction ID: {webhookEvent.TransactionId}");
        }

        // 5. Abonelik ödemesi mi? (dönem tahsilatları ayrı transaction_id ile gelir)
        if (webhookEvent.HasSubscription())
        {
            Console.WriteLine($"Abonelik {webhookEvent.GetSubscriptionId()}, dönem {webhookEvent.GetSubscriptionCycleNumber()}");
        }

        return Ok();
    }
    catch (TahsilatWebhookException ex)
    {
        // İmza doğrulaması başarısız ya da zaman damgası tolerans dışında
        return BadRequest(new { error = "Invalid signature" });
    }
}
```

Zaman toleransı `tolerance` parametresiyle ayarlanır:

```csharp
// Varsayılan: 5 dakika (WebhookHandler.DefaultTolerance)
WebhookHandler.ConstructEvent(payloadBytes, signature, secret);

// Daha geniş tolerans
WebhookHandler.ConstructEvent(payloadBytes, signature, secret, TimeSpan.FromMinutes(15));

// Zaman kontrolünü kapat (imza yine doğrulanır) — yalnız sunucu saatine güvenemiyorsanız
WebhookHandler.ConstructEvent(payloadBytes, signature, secret, WebhookHandler.DisableTimestampCheck);
```

> **1.2.0 davranış değişikliği:** Önceki sürümlerde `tolerance` verilmediğinde zaman kontrolü hiç yapılmıyordu. Artık varsayılan 5 dakikadır. Sunucu saatinizin doğru (NTP ile eşitlenmiş) olduğundan emin olun.

> **Not:** `IsSuccess()`, `IsFailed()`, `IsPending()`, `IsRefunded()`, `HasSubscription()` gibi extension metotları ile ödeme, işlem ve abonelik durumunu kolayca kontrol edebilirsiniz.



## Senkron Kullanım

Tüm servisler hem asenkron hem de senkron metotları destekler. Eski .NET Framework projelerinde async/await kullanamıyorsanız:

```csharp
// Senkron kullanım
var response = tahsilat.Payments.Create(request);
var transaction = tahsilat.Transactions.Retrieve(transactionId);
var customer = tahsilat.Customers.Create(customerRequest);
var page = tahsilat.Products.Search(new ProductSearchRequest { ProductName = "Premium" });
```

> Senkron ve asenkron metotlar aynı tipleri döner; `ResolvePreAuth` da `ResolvePreAuthAsync` gibi `ApiResponse<PreAuthResolveResponse>` döner.

## Güvenlik

- 🔒 Tüm API iletişimi **HTTPS** üzerinden zorunludur
- 🔑 API anahtarları `sk_test_` / `sk_live_` prefix kontrolü ile doğrulanır
- 🛡️ Webhook imzaları **HMAC-SHA256** ile doğrulanır
- ⏱️ Webhook **replay koruması** (timestamp toleransı)
- 🔐 Constant-time karşılaştırma ile **timing attack** koruması

## Ortam Ayrımı

SDK, API anahtarınızın prefix'ine göre ortamı otomatik belirler:

| Prefix | Ortam | API URL |
|--------|-------|---------|
| `sk_test_` | Sandbox | `https://api.sandbox.tahsilat.com/v1/` |
| `sk_live_` | Production | `https://api.tahsilat.com/v1/` |

## Lisans

MIT License - detaylar için LICENSE dosyasına bakın.

## Destek

- Dokümantasyon: [https://docs.tahsilat.com](https://docs.tahsilat.com)
- E-posta: info@tahsilat.com
