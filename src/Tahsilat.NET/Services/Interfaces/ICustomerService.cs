using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerResponse> CreateAsync(CustomerCreateRequest request, CancellationToken ct = default);
        Task<CustomerResponse> UpdateAsync(long customerId, CustomerUpdateRequest request, CancellationToken ct = default);
        Task<CustomerResponse> GetAsync(long customerId, CancellationToken ct = default);
        /// <summary>
        /// Searches customers. Pass null to list the first page without filters.
        /// </summary>
        Task<CustomerSearchResponse> SearchAsync(CustomerSearchRequest request = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(long customerId, CancellationToken ct = default);

        CustomerResponse Create(CustomerCreateRequest request);
        CustomerResponse Update(long customerId, CustomerUpdateRequest request);
        CustomerResponse Get(long customerId);
        CustomerSearchResponse Search(CustomerSearchRequest request = null);
        bool Delete(long customerId);
    }
}
