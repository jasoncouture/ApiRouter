using System.Net.Http;
using System.Threading.Tasks;

namespace ApiRouter.Core.Interfaces
{
    public interface IRequestHandlerModule
    {
        Task<bool> ShouldProcessRequest(HttpRequestMessage requestMessage, RequestRouterConfiguration configuration);
        Task<HttpResponseMessage> ProcessReuqest(HttpRequestMessage requestMessage, RequestRouterConfiguration configuration);
    }
}