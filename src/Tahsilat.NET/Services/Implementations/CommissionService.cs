using System.Collections.Generic;
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
    internal class CommissionService : BaseService, ICommissionService
    {
        public CommissionService(ITahsilatHttpClient http) : base(http) { }

        public async Task<List<CommissionResponse>> SearchAsync(CommissionSearchRequest request = null, CancellationToken cancellationToken = default)
        {
            var httpRequest = request != null
                ? CreateGet("pos/commissions", request)
                : CreateGet("pos/commissions");
            var response = await _http.SendAndReadAsync<ApiResponse<List<CommissionResponse>>>(httpRequest, cancellationToken).ConfigureAwait(false);
            return response?.Data;
        }

        public List<CommissionResponse> Search(CommissionSearchRequest request = null)
        {
            var httpRequest = request != null
                ? CreateGet("pos/commissions", request)
                : CreateGet("pos/commissions");
            var response = _http.SendAndRead<ApiResponse<List<CommissionResponse>>>(httpRequest);
            return response?.Data;
        }
    }
}
