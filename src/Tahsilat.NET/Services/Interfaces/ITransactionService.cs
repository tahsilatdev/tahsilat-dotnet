using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionResult> RetrieveAsync(long transactionId, CancellationToken cancellationToken = default);
        Task<PreAuthResolveResponse> ResolvePreAuthAsync(PreAuthResolveRequest request, CancellationToken ct = default);
        Task<ApiResponse<RefundResponse>> RefundAsync(RefundCreateRequest request, CancellationToken ct = default);

        TransactionResult Retrieve(long transactionId);
        PreAuthResolveResponse ResolvePreAuth(PreAuthResolveRequest request);
        ApiResponse<RefundResponse> Refund(RefundCreateRequest request);
    }
}
