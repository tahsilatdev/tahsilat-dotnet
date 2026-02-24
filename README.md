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
    options.TimeoutSeconds = 30; // Varsayılan: 30 saniye
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

### Ürün Oluşturma
```csharp
var request = new ProductCreateRequest
{
    ProductName = "Test Product",
    Price = 75900,
    Description = "Integration Test Product",
    StockCode = "TEST",
    Category = "TEST",
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

### İşlem Sorgulama
```csharp
var transaction = await client.Transactions.RetrieveAsync(78810412652494);

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
```

### İade İşlemi

```csharp
 var request = new RefundCreateRequest
 {
     TransactionId = 78810412652494,
     Amount = 1000, // Kısmi iade (10.00 TL)
     Description = "Müşteri talebi ile iade"
 };

var response = await tahsilat.Transactions.RefundAsync(request);
```

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
## Hata Yönetimi
```csharp

using Tahsilat.NET.Exceptions;

try
{
    var payment = await tahsilat.Payments.CreateAsync(new CreatePaymentRequest
    {
        Amount = 10000,
        Currency = "TRY"
    });
}
catch (TahsilatAuthenticationException ex)
{
    // Geçersiz API key (401)
    Console.WriteLine($"Kimlik doğrulama hatası: {ex.Message}");
    Console.WriteLine($"HTTP Durum Kodu: {ex.StatusCode}");
}
catch (TahsilatValidationException ex)
{
    // Geçersiz istek parametreleri (422)
    Console.WriteLine($"Validasyon hatası: {ex.Message}");
    Console.WriteLine($"Hata Kodu: {ex.ErrorCode}");
}
catch (TahsilatNotFoundException ex)
{
    // Kaynak bulunamadı (404)
    Console.WriteLine($"Bulunamadı: {ex.Message}");
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
    // Ağ hatası (bağlantı sorunu, timeout vb.)
    Console.WriteLine($"Ağ hatası: {ex.Message}");
}
catch (TahsilatApiException ex)
{
    // Diğer API hataları (5xx vb.)
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
| ├─ `TahsilatAuthenticationException` | Geçersiz API key (401) | `StatusCode` |
| ├─ `TahsilatValidationException` | Geçersiz istek parametreleri (422) | — |
| ├─ `TahsilatNotFoundException` | Kaynak bulunamadı (404) | — |
| ├─ `TahsilatPaymentException` | Ödeme işlemi hatası | — |
| ├─ `TahsilatRateLimitException` | İstek limiti aşıldı (429) | `RetryAfterSeconds` |
| ├─ `TahsilatNetworkException` | Ağ/bağlantı hatası | — |
| ├─ `TahsilatApiException` | Diğer API hataları | `StatusCode` |
| └─ `TahsilatWebhookException` | Webhook doğrulama hatası | — |


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
        // 3. Webhook event'i doğrula ve parse et
        var webhookEvent = WebhookHandler.ConstructEvent(payloadBytes, signature, "whsec_YOUR_WEBHOOK_SECRET");

        // 4. Ödeme durumuna göre işlem yap
        if (webhookEvent.IsSuccess())
        {
            // Ödeme başarılı
            Console.WriteLine($"Ödeme başarılı! Transaction ID: {webhookEvent.TransactionId}");
            Console.WriteLine($"Tutar: {webhookEvent.Amount} {webhookEvent.CurrencyCode}");
        }
        else if (webhookEvent.IsFailed())
        {
            // Ödeme başarısız
            Console.WriteLine($"Ödeme başarısız. Transaction ID: {webhookEvent.TransactionId}");
        }

        return Ok();
    }
    catch (TahsilatWebhookException ex)
    {
        // İmza doğrulaması başarısız
        return BadRequest(new { error = "Invalid signature" });
    }
}
```

> **Not:** `IsSuccess()`, `IsFailed()`, `IsPending()`, `IsRefunded()` gibi extension metotları ile ödeme ve işlem durumunu kolayca kontrol edebilirsiniz.



## Senkron Kullanım

Tüm servisler hem asenkron hem de senkron metotları destekler. Eski .NET Framework projelerinde async/await kullanamıyorsanız:

```csharp
// Senkron kullanım
var response = tahsilat.Payments.Create(request);
var transaction = tahsilat.Transactions.Retrieve(transactionId);
var customer = tahsilat.Customers.Create(customerRequest);
```

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
