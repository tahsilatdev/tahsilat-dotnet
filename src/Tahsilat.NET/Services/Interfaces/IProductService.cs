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
        /// <summary>
        /// Searches products. Pass null to list the first page without filters.
        /// </summary>
        Task<ProductSearchResponse> SearchAsync(ProductSearchRequest request = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(long productId, CancellationToken ct = default);

        /// <summary>
        /// Replaces the product's metadata with <paramref name="metadata"/>. The list
        /// is not merged: entries that are left out are removed, so resend the ones
        /// you want to keep. Up to 25 entries; key ^[a-z][a-z0-9_]*$ (max 20 chars),
        /// value max 200 chars. Returns the updated product.
        /// </summary>
        Task<ProductResponse> UpdateMetadataAsync(long productId, List<Dictionary<string, object>> metadata, CancellationToken ct = default);

        ProductResponse Create(ProductCreateRequest request);
        ProductResponse Update(long productId, ProductUpdateRequest request);
        ProductResponse Get(long productId);
        ProductSearchResponse Search(ProductSearchRequest request = null);
        bool Delete(long productId);
        ProductResponse UpdateMetadata(long productId, List<Dictionary<string, object>> metadata);
    }
}
