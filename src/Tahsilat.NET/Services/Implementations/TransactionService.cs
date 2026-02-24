using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Infrastructure.Http;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;
using Tahsilat.NET.Services.Interfaces;

namespace Tahsilat.NET.Services.Implementations
{
    internal class TransactionService : BaseService, ITransactionService
    {
        public TransactionService(ITahsilatHttpClient http) : base(http) { }

        public async Task<TransactionResult> RetrieveAsync(long transactionId, CancellationToken ct = default)
        {
            var httpRequest = CreateGet($"transaction/{transactionId}");
            var response = await _http.SendAndReadAsync<ApiResponse<TransactionResult>>(httpRequest, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<PreAuthResolveResponse> ResolvePreAuthAsync(PreAuthResolveRequest request, CancellationToken ct = default)
        {
            var httpRequest = CreatePost("transaction/resolve-pre-auth", request);
            var response = await _http.SendAndReadAsync<ApiResponse<PreAuthResolveResponse>>(httpRequest, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<ApiResponse<RefundResponse>> RefundAsync(RefundCreateRequest request, CancellationToken ct = default)
        {
            var httpRequest = CreatePost("transaction/refund", request);
            var response = await _http.SendAndReadAsync<ApiResponse<RefundResponse>>(httpRequest, ct).ConfigureAwait(false);
            return response;
        }

        public TransactionResult Retrieve(long transactionId)
        {
            var httpRequest = CreateGet($"transaction/{transactionId}");
            var response = _http.SendAndRead<ApiResponse<TransactionResult>>(httpRequest);
            return response?.Data;
        }

        public PreAuthResolveResponse ResolvePreAuth(PreAuthResolveRequest request)
        {
            var httpRequest = CreatePost("transaction/resolve-pre-auth", request);
            var response = _http.SendAndRead<ApiResponse<PreAuthResolveResponse>>(httpRequest);
            return response?.Data;
        }

        public ApiResponse<RefundResponse> Refund(RefundCreateRequest request)
        {
            var httpRequest = CreatePost("transaction/refund", request);
            var response = _http.SendAndRead<ApiResponse<RefundResponse>>(httpRequest);
            return response;
        }
    }
}
