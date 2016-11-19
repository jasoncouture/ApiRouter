using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRouter.Core.Interfaces
{
    public interface IRequestHandlerModule
    {
        Task<bool> ShouldProcessRequest(HttpRequestMessage requestMessage, RequestRouterConfigurationBase configuration, CancellationToken cancellationToken);
        Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, RequestRouterConfigurationBase configuration, CancellationToken cancellationToken);
    }
}