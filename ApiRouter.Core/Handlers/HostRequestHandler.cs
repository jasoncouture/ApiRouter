using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Interfaces;

namespace ApiRouter.Core.Handlers
{
    public class HostRequestHandler : RequestHandlerModuleBase<HostRouterConfiguration>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HostRequestHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, HostRouterConfiguration routerConfiguration,
            CancellationToken cancellationToken)
        {
            var endpoint = new DnsEndPoint(routerConfiguration.Host, routerConfiguration.Port ?? requestMessage.RequestUri.Port);
            var uri = CreateTargetUri(requestMessage.RequestUri, routerConfiguration, endpoint);
            requestMessage = PrepareRequestForForwarding(requestMessage, uri, routerConfiguration);
            var client = _httpClientFactory.GetHttpClient();
            try
            {
                return await client.SendAsync(requestMessage, cancellationToken);
            }
            catch
            {
                return requestMessage.CreateResponse(HttpStatusCode.BadGateway);
            }
            finally
            {
                _httpClientFactory.Release(client);
            }
        }
    }
}