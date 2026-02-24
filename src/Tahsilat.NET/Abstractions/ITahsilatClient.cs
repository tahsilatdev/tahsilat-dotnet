using Tahsilat.NET.Services.Interfaces;

namespace Tahsilat.NET.Abstractions
{
    public interface ITahsilatClient
    {
        IPaymentService Payments { get; }
        ITransactionService Transactions { get; }
        IRefundService Refunds { get; }
        ICustomerService Customers { get; }
        IProductService Products { get; }
        IBinLookupService BinLookup { get; }
        ICommissionService Commissions { get; }
    }
}
