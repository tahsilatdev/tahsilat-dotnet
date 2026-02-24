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
        Task<List<CustomerResponse>> SearchAsync(string keyword, CancellationToken ct = default);
        Task<bool> DeleteAsync(long customerId, CancellationToken ct = default);

        CustomerResponse Create(CustomerCreateRequest request);
        CustomerResponse Update(long customerId, CustomerUpdateRequest request);
        CustomerResponse Get(long customerId);
        List<CustomerResponse> Search(string keyword);
        bool Delete(long customerId);
    }
}
