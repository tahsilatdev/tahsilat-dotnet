using System.Threading;
using System.Threading.Tasks;
using Tahsilat.NET.Models.Requests;
using Tahsilat.NET.Models.Responses;

namespace Tahsilat.NET.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreateAsync(PaymentCreateRequest request, CancellationToken cancellationToken = default);
        PaymentResponse Create(PaymentCreateRequest request);
    }
}
