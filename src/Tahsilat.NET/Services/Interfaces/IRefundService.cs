using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Services.Interfaces
{
    public interface IRefundService
    {
        Task<ApiResponse<RefundResponse>> CreateAsync(RefundCreateRequest request, CancellationToken cancellationToken = default);
        ApiResponse<RefundResponse> Create(RefundCreateRequest request);
    }
}
