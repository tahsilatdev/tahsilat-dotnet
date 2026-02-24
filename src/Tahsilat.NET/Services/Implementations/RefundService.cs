using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Infrastructure.Http;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;
using Tahsilat.NET.Services.Interfaces;
using Tahsilat.NET.Models.Common;

namespace Tahsilat.NET.Services.Implementations
{
    internal class RefundService : BaseService, IRefundService
    {
        public RefundService(ITahsilatHttpClient http) : base(http) { }

        public Task<ApiResponse<RefundResponse>> CreateAsync(RefundCreateRequest request, CancellationToken cancellationToken = default)
        {
            var httpRequest = CreatePost("refund/create", request);
            return _http.SendAndReadAsync<ApiResponse<RefundResponse>>(httpRequest, cancellationToken);
        }

        public ApiResponse<RefundResponse> Create(RefundCreateRequest request)
        {
            var httpRequest = CreatePost("refund/create", request);
            return _http.SendAndRead<ApiResponse<RefundResponse>>(httpRequest);
        }
    }
}
