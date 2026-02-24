using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Infrastructure.Http;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;
using Tahsilat.NET.Services.Interfaces;

namespace Tahsilat.NET.Services.Implementations
{
    internal class PaymentService : BaseService, IPaymentService
    {
        public PaymentService(ITahsilatHttpClient http) : base(http) { }

        public async Task<PaymentResponse> CreateAsync(PaymentCreateRequest request, CancellationToken cancellationToken = default)
        {
            var httpRequest = CreatePost("payment/3ds", request);
            var apiResponse = await _http.SendAndReadAsync<ApiResponse<PaymentResponse>>(httpRequest, cancellationToken).ConfigureAwait(false);
            return apiResponse?.Data;
        }

        public PaymentResponse Create(PaymentCreateRequest request)
        {
            var httpRequest = CreatePost("payment/3ds", request);
            var apiResponse = _http.SendAndRead<ApiResponse<PaymentResponse>>(httpRequest);
            return apiResponse?.Data;
        }
    }
}
