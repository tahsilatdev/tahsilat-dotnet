using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponse> CreateAsync(ProductCreateRequest request, CancellationToken ct = default);
        Task<ProductResponse> UpdateAsync(long productId, ProductUpdateRequest request, CancellationToken ct = default);
        Task<ProductResponse> GetAsync(long productId, CancellationToken ct = default);
        Task<List<ProductResponse>> SearchAsync(string keyword, CancellationToken ct = default);
        Task<bool> DeleteAsync(long productId, CancellationToken ct = default);

        ProductResponse Create(ProductCreateRequest request);
        ProductResponse Update(long productId, ProductUpdateRequest request);
        ProductResponse Get(long productId);
        List<ProductResponse> Search(string keyword);
        bool Delete(long productId);
    }
}
