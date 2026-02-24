using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Common;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Services.Interfaces
{
    public interface IBinLookupService
    {
        Task<BinLookupResponse> DetailAsync(long bin_number, CancellationToken cancellationToken = default);
        BinLookupResponse Detail(long bin_number);
    }
}
