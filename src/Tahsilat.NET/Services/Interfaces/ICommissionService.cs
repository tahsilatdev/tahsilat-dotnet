using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Services.Interfaces
{
    public interface ICommissionService
    {
        Task<List<CommissionResponse>> SearchAsync(CommissionSearchRequest request = null, CancellationToken cancellationToken = default);
        List<CommissionResponse> Search(CommissionSearchRequest request = null);
    }
}
