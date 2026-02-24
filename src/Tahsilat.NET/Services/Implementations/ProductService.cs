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
    internal class ProductService : BaseService, IProductService
    {
        public ProductService(ITahsilatHttpClient http) : base(http) { }

        public async Task<ProductResponse> CreateAsync(ProductCreateRequest request, CancellationToken ct = default)
        {
            var req = CreatePost("products", request);
            var response = await _http.SendAndReadAsync<ApiResponse<ProductResponse>>(req, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<ProductResponse> UpdateAsync(long productId, ProductUpdateRequest request, CancellationToken ct = default)
        {
            var req = CreatePost($"products/{productId}", request);
            var response = await _http.SendAndReadAsync<ApiResponse<ProductResponse>>(req, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<ProductResponse> GetAsync(long productId, CancellationToken ct = default)
        {
            var req = CreateGet($"products/{productId}");
            var response = await _http.SendAndReadAsync<ApiResponse<ProductResponse>>(req, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<List<ProductResponse>> SearchAsync(string keyword, CancellationToken ct = default)
        {
            var req = CreateGet($"products/search?keyword={keyword}");
            var response = await _http.SendAndReadAsync<ApiResponse<List<ProductResponse>>>(req, ct).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<bool> DeleteAsync(long productId, CancellationToken ct = default)
        {
            var req = CreateDelete($"products/{productId}");
            var response = await _http.SendAndReadAsync<ApiResponse<object>>(req, ct).ConfigureAwait(false);
            return response?.Status ?? false;
        }

        // Sync wrappers

        public ProductResponse Create(ProductCreateRequest request)
        {
            var req = CreatePost("products", request);
            var response = _http.SendAndRead<ApiResponse<ProductResponse>>(req);
            return response?.Data;
        }

        public ProductResponse Update(long productId, ProductUpdateRequest request)
        {
            var req = CreatePost($"products/{productId}", request);
            var response = _http.SendAndRead<ApiResponse<ProductResponse>>(req);
            return response?.Data;
        }

        public ProductResponse Get(long productId)
        {
            var req = CreateGet($"products/{productId}");
            var response = _http.SendAndRead<ApiResponse<ProductResponse>>(req);
            return response?.Data;
        }

        public List<ProductResponse> Search(string keyword)
        {
            var req = CreateGet($"products/search?keyword={keyword}");
            var response = _http.SendAndRead<ApiResponse<List<ProductResponse>>>(req);
            return response?.Data;
        }

        public bool Delete(long productId)
        {
            var req = CreateDelete($"products/{productId}");
            var response = _http.SendAndRead<ApiResponse<object>>(req);
            return response?.Status ?? false;
        }
    }
}
