using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tahsilat.NET.Infrastructure.Http
{
    internal interface ITahsilatHttpClient
    {
        Task<TResponse> SendAndReadAsync<TResponse>(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default);

        TResponse SendAndRead<TResponse>(HttpRequestMessage request);
    }
}
