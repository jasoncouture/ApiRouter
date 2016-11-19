using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRouter.Core.Handlers
{
    public class ResponseCodeRequestHandler : RequestHandlerModuleBase<ResponseCodeRouterConfiguration>
    {
        protected override Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, ResponseCodeRouterConfiguration configuration,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(requestMessage.CreateResponse(configuration.StatusCode));
        }
    }
}