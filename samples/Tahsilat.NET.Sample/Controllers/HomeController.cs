using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using Tahsilat.NET.Abstractions;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Sample.Models;

namespace Tahsilat.NET.Sample.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    //private readonly ITahsilatClient _tahsilat;

    public HomeController(ILogger<HomeController> logger/*, ITahsilatClient tahsilat*/)
    {
        _logger = logger;
        //_tahsilat = tahsilat;
    }

    public IActionResult Index()
    {
        // Display a static product for demo
        ViewBag.Product = "iPhone 15 Pro Max";
        ViewBag.Price = 85000;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        try
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");

            //1. Doğrudan Ödeme
            //var request = new PaymentCreateRequest
            //{
            //    Currency = "TRY",
            //    Amount = 500000,
            //    Metadata = new()
            //    {
            //        new Dictionary<string, object>
            //        {
            //            ["customer_type"] = "premium",
            //            ["special_id"] = 123456
            //        }
            //    }
            //};

            //var response = await tahsilat.Payments.CreateAsync(request);

            //Ürün Oluşturma
            //var productRequest = new ProductCreateRequest
            //{
            //    ProductName = "iPhone 15 Pro Max",
            //    Price = 50000,
            //    Description = "256GB Titanium"
            //};

            //var productResponse = await tahsilat.Products.CreateAsync(productRequest);

            ////3. Ürün ID’leri ile Ödeme

            //var paymentRequest = new PaymentCreateRequest
            //{
            //    Currency = "TRY",
            //    Amount = 50000,
            //    ProductIds = new List<long>
            //    {
            //        84920468860151,
            //        55437751141488
            //    },
            //    CustomerId = 20585467989184,
            //    Metadata = new()
            //    {
            //        new Dictionary<string, object>
            //        {
            //            ["order_id"] = 123456,
            //            ["customer_type"] = "premium"
            //        },
            //        new Dictionary<string, object>
            //        {
            //            ["created"] = "Subat2026",
            //            ["source"] = "Tahsilat-dotnet-test"
            //        }
            //    },
            //    RedirectUrl = $"{Request.Scheme}://{Request.Host}/Home/PaymentResult"
            //};

            //var response = await tahsilat.Payments.CreateAsync(paymentRequest);

            //if (!string.IsNullOrEmpty(response.PaymentPageUrl))
            //{
            //    // Redirect user to the hosted payment page
            //    return Redirect(response.PaymentPageUrl);
            //}

            ////2. Doğrudan Ürün Bilgileri ile Ödeme
            //var request = new PaymentCreateRequest
            //{
            //    Currency = "TRY",
            //    Amount = 70000,
            //    RedirectUrl = "https://example.com/payment/callback",
            //    Products = new List<ProductItem>
            //    {
            //        new ProductItem
            //        {
            //            ProductName = "Product1",
            //            Price = 50000,
            //            Description = "Test Product"
            //        },
            //        new ProductItem
            //        {
            //            ProductName = "Product2",
            //            Price = 10000,
            //            Description = "Test Product"
            //        },
            //        new ProductItem
            //        {
            //            ProductName = "Product3",
            //            Price = 10000,
            //            Description = "Test Product"
            //        }
            //    },
            //    Metadata = new()
            //    {
            //        new Dictionary<string, object>
            //        {
            //            ["order_id"] = 123456,
            //            ["customer_type"] = "premium"
            //        },
            //        new Dictionary<string, object>
            //        {
            //            ["created"] = "Subat2026",
            //            ["source"] = "Tahsilat-dotnet-test"
            //        }
            //    },
            //    Description = "Integration Test Product"
            //};


            //var response = await tahsilat.Payments.CreateAsync(request);

            //if (!string.IsNullOrEmpty(response.PaymentPageUrl))
            //{
            //    // Redirect user to the hosted payment page
            //    return Redirect(response.PaymentPageUrl);
            //}


            //Ürün ID’leri ile Ödeme
            //var paymnetRequest = new PaymentCreateRequest
            //{
            //    Currency = "TRY",
            //    Amount = 5000,
            //    ProductIds = new List<long>
            //    {
            //        12313213213213
            //    },
            //    Metadata = new()
            //    {
            //        new Dictionary<string, object>
            //        {
            //            ["order_id"] = 123456,
            //            ["customer_type"] = "premium"
            //        },
            //        new Dictionary<string, object>
            //        {
            //            ["test"] = "Subat2026",
            //            ["asd"] = "Tahsilat-dotnet-test"
            //        }
            //    }
            //};

            //Müşteri ile İlişkilendirilmiş Ödeme
            //var customerRequest = new CustomerCreateRequest
            //{
            //    Address = "Sarıyer/İstanbul",
            //    City = "İstanbul",
            //    Country = "TR",
            //    District = "Sarıyer",
            //    Email = "muhammetgcl@yandex.com",
            //    LastName = "SDK Test",
            //    Name = "NET TEST",
            //    Phone = "5434589632",
            //    ZipCode = "34000",
            //    Metadata = new()
            //    {
            //        new Dictionary<string, object>
            //        {
            //            ["customer_type"] = "premium",
            //            ["special_id"] = 123456
            //        }
            //    }
            //};

            //var customerResponse = await tahsilat.Customers.CreateAsync(customerRequest);

            //var productRequest = new ProductCreateRequest
            //{
            //    ProductName = "iPhone 15 Pro Max",
            //    Price = 500000,
            //    Description = "256GB Titanium"
            //};

            //var productResponse = await tahsilat.Products.CreateAsync(productRequest);


            //var request = new PaymentCreateRequest
            //{
            //    Currency = "TRY",
            //    Amount = 500000,
            //    CustomerId = customerResponse.Id,
            //    ProductIds = new List<long>
            //    {
            //        productResponse.Id
            //    },
            //    RedirectUrl = $"{Request.Scheme}://{Request.Host}/Home/PaymentResult"
            //};

            //var response = await tahsilat.Payments.CreateAsync(request);

            //Doğrudan Ödeme
            var request = new PaymentCreateRequest
            {
                Currency = "TRY",
                Amount = 500000,
                Metadata = new()
                {
                    new Dictionary<string, object>
                    {
                        ["customer_type"] = "premium",
                        ["special_id"] = 123456
                    }
                },
                RedirectUrl = $"{Request.Scheme}://{Request.Host}/Home/PaymentResult"
            };

            var response = await tahsilat.Payments.CreateAsync(request);


            if (!string.IsNullOrEmpty(response.PaymentPageUrl))
            {
                // Redirect user to the hosted payment page
                return Redirect(response.PaymentPageUrl);
            }

            ViewBag.Error = "Payment page URL not returned.";
            return View("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment.");
            ViewBag.Error = ex.Message;
            return View("Index");
        }
    }

    [HttpGet]
    public async Task<IActionResult> PaymentResult([FromQuery(Name = "transaction_id")] long transactionId)
    {
        if (transactionId <= 0)
        {
            ViewBag.Error = "Geçersiz transaction ID.";
            return View();
        }

        try
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var transaction = await tahsilat.Transactions.RetrieveAsync(transactionId);

            ViewBag.TransactionId = transactionId;
            ViewBag.Transaction = transaction;

            if (transaction.PaymentStatus == 1)
            {
                ViewBag.Success = true;
            }
            else
            {
                ViewBag.Success = false;
                ViewBag.Error = $"Ödeme başarısız. Durum: {transaction.PaymentStatusText}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme sonucu doğrulama hatası.");
            ViewBag.Error = ex.Message;
        }

        return View();
    }

    public async Task<IActionResult> Callback(long? transaction_id)
    {
        if (!transaction_id.HasValue)
        {
            ViewBag.Message = "Transaction ID is missing.";
            ViewBag.Success = false;
            return View();
        }

        try
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var transaction = await tahsilat.Transactions.RetrieveAsync(transaction_id.Value);

            if (transaction.PaymentStatus == 1) // 1 = Success
            {
                ViewBag.Success = true;
                ViewBag.Message = $"Payment Successful! Ref: {transaction.TransactionId}";
                ViewBag.Details = transaction;
            }
            else
            {
                ViewBag.Success = false;
                ViewBag.Message = $"Payment Failed or Pending. Status: {transaction.PaymentStatusText}";
                ViewBag.Details = transaction;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying transaction.");
            ViewBag.Success = false;
            ViewBag.Message = "Error verifying transaction: " + ex.Message;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

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
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var result = await tahsilat.BinLookup.DetailAsync(binNumber);
            ViewBag.Result = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bin Sorgulama hatası.");
            ViewBag.Error = ex.Message;
        }

        return View();
    }

    [HttpGet]
    public IActionResult Installments()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Installments(int binNumber, decimal? price)
    {
        try
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var request = new CommissionSearchRequest { BinNumber = binNumber };
            var result = await tahsilat.Commissions.SearchAsync(request);

            ViewBag.Result = result;
            ViewBag.Price = price;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Taksit Sorgulama hatası.");
            ViewBag.Error = ex.Message;
        }

        return View();
    }

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
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var result = await tahsilat.Products.CreateAsync(model);
            ViewBag.Result = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün oluşturma hatası.");
            ViewBag.Error = ex.Message;
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult CreateCustomer()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CustomerCreateRequest model)
    {
        ModelState.Remove("Metadata");
        if (!ModelState.IsValid)
        {
            ViewBag.Error = "Lütfen tüm alanları doldurunuz.";
            var errors = ModelState.Values
                           .SelectMany(v => v.Errors)
                           .Select(e => e.ErrorMessage)
                           .ToList();
            return View(model);
        }
        try
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var result = await tahsilat.Customers.CreateAsync(model);
            ViewBag.Result = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müşteri oluşturma hatası.");
            ViewBag.Error = ex.Message;
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
        if (transactionId <= 0)
        {
            ViewBag.Error = "Lütfen geçerli bir Transaction ID giriniz.";
            return View();
        }
        try
        {
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var req = new PreAuthResolveRequest
            {
                TransactionId = transactionId,
                Status = status
            };
            var result = await tahsilat.Transactions.ResolvePreAuthAsync(req);
            ViewBag.Result = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pre-Auth Resolve hatası.");
            ViewBag.Error = ex.Message;
        }
        return View();
    }

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
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var result = await tahsilat.Transactions.RefundAsync(model);
            ViewBag.Result = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İade işlemi hatası.");
            ViewBag.Error = ex.Message;
        }
        return View(model);
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
            var tahsilat = new TahsilatClient("sk_test_YOUR_SECRET_KEY");
            var result = await tahsilat.Transactions.RetrieveAsync(transactionId);
            ViewBag.Result = result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlem sonucu sorgulama hatası");
            ViewBag.Error = ex.Message;
        }

        return View();
    }
}
