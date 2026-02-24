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
    internal class CustomerService : BaseService, ICustomerService
    {
        public CustomerService(ITahsilatHttpClient http) : base(http) { }

        public async Task<CustomerResponse> CreateAsync(CustomerCreateRequest request, CancellationToken ct = default)
        {
            var httpRequest = CreatePost("customers", request);
            var response = await _http.SendAndReadAsync<ApiResponse<CustomerResponse>>(httpRequest, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<CustomerResponse> UpdateAsync(long customerId, CustomerUpdateRequest request, CancellationToken ct = default)
        {
            var httpRequest = CreatePost($"customers/{customerId}", request);
            var response = await _http.SendAndReadAsync<ApiResponse<CustomerResponse>>(httpRequest, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<CustomerResponse> GetAsync(long customerId, CancellationToken ct = default)
        {
            var httpRequest = CreateGet($"customers/{customerId}");
            var response = await _http.SendAndReadAsync<ApiResponse<CustomerResponse>>(httpRequest, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<List<CustomerResponse>> SearchAsync(string keyword, CancellationToken ct = default)
        {
            var req = CreateGet($"customers/search?sources={keyword}");
            var response = await _http.SendAndReadAsync<ApiResponse<List<CustomerResponse>>>(req, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<bool> DeleteAsync(long customerId, CancellationToken ct = default)
        {
            var httpRequest = CreateDelete($"customers/{customerId}");
            var response = await _http.SendAndReadAsync<ApiResponse<object>>(httpRequest, ct).ConfigureAwait(false);
            return response?.Status ?? false;
        }

        // Sync wrappers

        public CustomerResponse Create(CustomerCreateRequest request)
        {
            var httpRequest = CreatePost("customers", request);
            var response = _http.SendAndRead<ApiResponse<CustomerResponse>>(httpRequest);
            return response?.Data;
        }

        public CustomerResponse Update(long customerId, CustomerUpdateRequest request)
        {
            var httpRequest = CreatePost($"customers/{customerId}", request);
            var response = _http.SendAndRead<ApiResponse<CustomerResponse>>(httpRequest);
            return response?.Data;
        }

        public CustomerResponse Get(long customerId)
        {
            var httpRequest = CreateGet($"customers/{customerId}");
            var response = _http.SendAndRead<ApiResponse<CustomerResponse>>(httpRequest);
            return response?.Data;
        }

        public List<CustomerResponse> Search(string keyword)
        {
            var req = CreateGet($"customers/search?sources={keyword}");
            var response = _http.SendAndRead<ApiResponse<List<CustomerResponse>>>(req);
            return response?.Data;
        }

        public bool Delete(long customerId)
        {
            var httpRequest = CreateDelete($"customers/{customerId}");
            var response = _http.SendAndRead<ApiResponse<object>>(httpRequest);
            return response?.Status ?? false;
        }
    }
}
