using System;
using Tahsilat.NET.Abstractions;
using Tahsilat.NET.Configuration;
using Tahsilat.NET.Infrastructure.Http;
using Tahsilat.NET.Services.Implementations;
using Tahsilat.NET.Services.Interfaces;

namespace Tahsilat.NET
{
    public sealed class TahsilatClient : ITahsilatClient, IDisposable
    {
        public IPaymentService Payments { get; }
        public ITransactionService Transactions { get; }
        public IRefundService Refunds { get; }
        public ICustomerService Customers { get; }
        public IProductService Products { get; }
        public IBinLookupService BinLookup { get; }
        public ICommissionService Commissions { get; }

        private readonly TahsilatHttpClient _http;
        private bool _disposed;

        public TahsilatClient(string secretKey)
            : this(new TahsilatClientOptions { ApiKey = secretKey })
        {
        }

        public TahsilatClient(TahsilatClientOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _http = new TahsilatHttpClient(options);

            Payments = new PaymentService(_http);
            Transactions = new TransactionService(_http);
            Refunds = new RefundService(_http);
            Customers = new CustomerService(_http);
            Products = new ProductService(_http);
            BinLookup = new BinLookupService(_http);
            Commissions = new CommissionService(_http);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _http?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
