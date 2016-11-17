using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRouter.Core
{
    public interface IServiceParser
    {
        Task<RequestRouterConfiguration> GetServiceAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}