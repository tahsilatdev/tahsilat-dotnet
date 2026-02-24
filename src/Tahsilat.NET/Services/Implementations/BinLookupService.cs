using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Infrastructure.Http;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;
using Tahsilat.NET.Services.Interfaces;

namespace Tahsilat.NET.Services.Implementations
{
    internal class BinLookupService : BaseService, IBinLookupService
    {
        public BinLookupService(ITahsilatHttpClient http) : base(http) { }

        public async Task<BinLookupResponse> DetailAsync(long bin_number, CancellationToken cancellationToken = default)
        {
            var httpRequest = CreateGet($"bin-lookup?bin_number={bin_number}");
            var response = await _http.SendAndReadAsync<ApiResponse<BinLookupResponse>>(httpRequest, cancellationToken).ConfigureAwait(false);
            return response?.Data;
        }

        public BinLookupResponse Detail(long bin_number)
        {
            var httpRequest = CreateGet($"bin-lookup?bin_number={bin_number}");
            var response = _http.SendAndRead<ApiResponse<BinLookupResponse>>(httpRequest);
            return response?.Data;
        }
    }
}
