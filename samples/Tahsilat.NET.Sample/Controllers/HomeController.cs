using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using Tahsilat.NET.Abstractions;
using Tahsilat.NET.Exceptions;
using Tahsilat.NET.Extensions;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;
using Tahsilat.NET.Sample.Models;

namespace Tahsilat.NET.Sample.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ITahsilatClient _tahsilat;

    public HomeController(ILogger<HomeController> logger, ITahsilatClient tahsilat)
    {
        _logger = logger;
        _tahsilat = tahsilat;
    }

    // ---------------------------------------------------------------- Mağaza demo

    /// <summary>
    /// Mağaza: sandbox'taki kayıtlı ürünler listelenir, her biri doğrudan satın alınabilir.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var products = new List<ProductResponse>();
        try
        {
            var page = await _tahsilat.Products.SearchAsync(new ProductSearchRequest { Limit = 50 });
            products = page?.Products ?? new List<ProductResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürünler listelenemedi.");
            ViewBag.Error = Describe(ex);
        }

        return View(products);
    }

    /// <summary>
    /// Seçilen kayıtlı ürün için 3DS ödeme başlatır ve müşteriyi ödeme sayfasına
    /// yönlendirir. Ürün abonelik ürünüyse ilk 3D ödeme tamamlanınca abonelik
    /// otomatik oluşur.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Checkout(long productId)
    {
        try
        {
            var product = await _tahsilat.Products.GetAsync(productId);

            var payment = await _tahsilat.Payments.CreateAsync(new PaymentCreateRequest
            {
                Currency = string.IsNullOrEmpty(product.CurrencyCode) ? "TRY" : product.CurrencyCode,
                Amount = product.Price,                       // ürün toplamıyla eşleşmeli
                ProductIds = new List<long> { product.Id },
                Description = $"Sample mağaza: {product.ProductName}",
                Metadata = new()
                {
                    new Dictionary<string, object> { ["source"] = "tahsilat-dotnet sample", ["product_id"] = product.Id }
                },
                RedirectUrl = Url.Action("PaymentResult", "Home", null, Request.Scheme)
            });

            if (!string.IsNullOrEmpty(payment.PaymentPageUrl))
                return Redirect(payment.PaymentPageUrl);

            TempData["Error"] = "Ödeme sayfası adresi dönmedi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme oluşturma hatası.");
            TempData["Error"] = Describe(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// RedirectUrl'e dönüş. Yönlendirme ödeme onayı değildir; sonuç API'den sorgulanır.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> PaymentResult([FromQuery(Name = "transaction_id")] long transactionId)
    {
        try
        {
            var transaction = await _tahsilat.Transactions.RetrieveAsync(transactionId);

            ViewBag.TransactionId = transactionId;
            ViewBag.Transaction = transaction;
            ViewBag.Success = transaction.IsSuccess();
            if (!transaction.IsSuccess())
                ViewBag.Error = $"Ödeme başarısız. Durum: {transaction.PaymentStatusText}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme sonucu doğrulama hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View();
    }

    public async Task<IActionResult> Callback(long? transaction_id)
    {
        try
        {
            var transaction = await _tahsilat.Transactions.RetrieveAsync(transaction_id ?? 0);

            ViewBag.Success = transaction.IsSuccess();
            ViewBag.Message = transaction.IsSuccess()
                ? $"Payment Successful! Ref: {transaction.TransactionId}"
                : $"Payment Failed or Pending. Status: {transaction.PaymentStatusText}";
            ViewBag.Details = transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying transaction.");
            ViewBag.Success = false;
            ViewBag.Message = "Error verifying transaction: " + Describe(ex);
        }

        return View();
    }

    // ---------------------------------------------------------------- Ödeme başlatma

    [HttpGet]
    public IActionResult StartPayment()
    {
        return View(new PaymentFormModel());
    }

    /// <summary>
    /// Kayıtlı ürün ID'leri ile ödeme. Abonelik ürünü içeriyorsa ilk 3D ödeme
    /// tamamlanınca abonelik otomatik oluşur.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StartPayment(PaymentFormModel model)
    {
        try
        {
            var request = new PaymentCreateRequest
            {
                Amount = model.Amount,
                Currency = model.Currency,
                CustomerId = model.CustomerId,
                ProductIds = ParseIds(model.ProductIds),
                SubscriptionCredentialId = model.SubscriptionCredentialId,
                PreAuth = model.PreAuth,
                Description = model.Description,
                Metadata = ParseMetadata(model.Metadata),
                RedirectUrl = Url.Action("PaymentResult", "Home", null, Request.Scheme)
            };

            var payment = await _tahsilat.Payments.CreateAsync(request);
            ViewBag.Result = payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme başlatma hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View(model);
    }

    // ---------------------------------------------------------------- Sorgular

    [HttpGet]
    public IActionResult BinLookup()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BinLookup(long binNumber)
    {
        try
        {
            ViewBag.Result = await _tahsilat.BinLookup.DetailAsync(binNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bin Sorgulama hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View();
    }

    [HttpGet]
    public IActionResult Installments()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Installments(int? binNumber, decimal? price)
    {
        try
        {
            // BIN verilirse o karta uygulanacak oranlar, verilmezse üye işyerinin
            // tüm aktif POS'larının oranları döner.
            ViewBag.Result = await _tahsilat.Commissions.SearchAsync(
                binNumber.HasValue ? new CommissionSearchRequest { BinNumber = binNumber } : null);
            ViewBag.Price = price;
            ViewBag.BinNumber = binNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Taksit Sorgulama hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View();
    }

    [HttpGet]
    public IActionResult TransactionResult()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TransactionResult(long transactionId)
    {
        try
        {
            ViewBag.Result = await _tahsilat.Transactions.RetrieveAsync(transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlem sonucu sorgulama hatası");
            ViewBag.Error = Describe(ex);
        }

        return View();
    }

    // ---------------------------------------------------------------- Müşteri

    [HttpGet]
    public IActionResult CreateCustomer()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CustomerCreateRequest model)
    {
        try
        {
            ViewBag.Result = await _tahsilat.Customers.CreateAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müşteri oluşturma hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult SearchCustomers()
    {
        return View(new CustomerSearchRequest { Limit = 20 });
    }

    [HttpPost]
    public async Task<IActionResult> SearchCustomers(CustomerSearchRequest model)
    {
        try
        {
            ViewBag.Result = await _tahsilat.Customers.SearchAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müşteri arama hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCustomer(long customerId)
    {
        try
        {
            var deleted = await _tahsilat.Customers.DeleteAsync(customerId);
            TempData["Message"] = deleted ? $"Müşteri {customerId} silindi." : $"Müşteri {customerId} silinemedi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müşteri silme hatası.");
            TempData["Error"] = Describe(ex);
        }

        return RedirectToAction(nameof(SearchCustomers));
    }

    // ---------------------------------------------------------------- Ürün

    [HttpGet]
    public IActionResult CreateProduct()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductCreateRequest model)
    {
        try
        {
            // Abonelik kutusu işaretli değilse alanları hiç gönderme.
            if (model.IsRecurring != true)
            {
                model.IsRecurring = null;
                model.BillingInterval = null;
                model.IntervalCount = null;
            }

            ViewBag.Result = await _tahsilat.Products.CreateAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün oluşturma hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult SearchProducts()
    {
        return View(new ProductSearchRequest { Limit = 20 });
    }

    [HttpPost]
    public async Task<IActionResult> SearchProducts(ProductSearchRequest model)
    {
        try
        {
            // Fiyat girilmediyse financial[] bloğunu gönderme.
            if (model.Financial != null && model.Financial.Price == null)
                model.Financial = null;

            ViewBag.Result = await _tahsilat.Products.SearchAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün arama hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(long productId)
    {
        try
        {
            var deleted = await _tahsilat.Products.DeleteAsync(productId);
            TempData["Message"] = deleted ? $"Ürün {productId} silindi." : $"Ürün {productId} silinemedi.";
        }
        catch (TahsilatConflictException ex)
        {
            // Aktif aboneliğe bağlı ürün silinemez (409).
            TempData["Error"] = "Çakışma: " + Describe(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün silme hatası.");
            TempData["Error"] = Describe(ex);
        }

        return RedirectToAction(nameof(SearchProducts));
    }

    [HttpGet]
    public IActionResult UpdateProductMetadata(long? productId)
    {
        return View(new MetadataFormModel { ProductId = productId ?? 0 });
    }

    /// <summary>
    /// Gönderilen liste ürünün mevcut metadata'sının tamamının yerine geçer.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateProductMetadata(MetadataFormModel model)
    {
        try
        {
            var metadata = ParseMetadata(model.Metadata) ?? new List<Dictionary<string, object>>();
            ViewBag.Result = await _tahsilat.Products.UpdateMetadataAsync(model.ProductId, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metadata güncelleme hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View(model);
    }

    // ---------------------------------------------------------------- İade / Ön provizyon

    [HttpGet]
    public IActionResult Refund()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Refund(RefundCreateRequest model)
    {
        try
        {
            ViewBag.Result = await _tahsilat.Transactions.RefundAsync(model);
        }
        catch (TahsilatApiException ex) when (ex.StatusCode == 200)
        {
            // 1.2.0: banka iadeyi reddederse Status=false dönmez, exception fırlar.
            ViewBag.Rejected = true;
            ViewBag.Error = $"İade reddedildi: {ex.Message} (kod: {ex.ErrorCode})";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İade işlemi hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult PreAuthResolve()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> PreAuthResolve(long transactionId, bool status)
    {
        try
        {
            ViewBag.Result = await _tahsilat.Transactions.ResolvePreAuthAsync(new PreAuthResolveRequest
            {
                TransactionId = transactionId,
                Status = status
            });
        }
        catch (TahsilatApiException ex) when (ex.StatusCode == 200)
        {
            ViewBag.Rejected = true;
            ViewBag.Error = $"İşlem reddedildi: {ex.Message} (kod: {ex.ErrorCode})";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pre-Auth Resolve hatası.");
            ViewBag.Error = Describe(ex);
        }

        return View();
    }

    // ---------------------------------------------------------------- Diğer

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // ---------------------------------------------------------------- Yardımcılar

    /// <summary>
    /// SDK exception'larını kullanıcıya okunur şekilde yazar: tür, hata kodu,
    /// alan bazlı doğrulama hataları, ağ hatalarında iç exception.
    /// </summary>
    private static string Describe(Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append('[').Append(ex.GetType().Name).Append("] ").Append(ex.Message);

        if (ex is TahsilatException te && !string.IsNullOrEmpty(te.ErrorCode))
            sb.Append(" (error_code: ").Append(te.ErrorCode).Append(')');

        if (ex is TahsilatValidationException ve && ve.Errors.Count > 0)
        {
            foreach (var field in ve.Errors)
                sb.Append(" • ").Append(field.Key).Append(": ").Append(string.Join(", ", field.Value));
        }

        if (ex is TahsilatNetworkException ne && ne.InnerException != null)
            sb.Append(" — ").Append(ne.InnerException.GetType().Name).Append(": ").Append(ne.InnerException.Message);

        return sb.ToString();
    }

    private static List<long>? ParseIds(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        return text.Split(new[] { ',', ';', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.Parse(s.Trim()))
            .ToList();
    }

    /// <summary>Satır başına "key=value" metnini SDK'nın metadata biçimine çevirir.</summary>
    private static List<Dictionary<string, object>>? ParseMetadata(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var list = new List<Dictionary<string, object>>();
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('=', 2);
            if (parts.Length != 2) continue;
            list.Add(new Dictionary<string, object> { [parts[0].Trim()] = parts[1].Trim() });
        }

        return list.Count == 0 ? null : list;
    }
}
