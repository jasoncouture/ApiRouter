using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRouter.Core.Interfaces
{
    public interface IRequestHandler
    {
        Task<HttpResponseMessage> HandleRequest(HttpRequestMessage request, RequestRouterConfigurationBase configuration, CancellationToken cancellationToken);
    }
}