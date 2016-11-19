using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;

namespace ApiRouter.Core.Handlers
{
    public class ConsulServiceRequestHandler : RequestHandlerModuleBase<ConsulServiceRouterConfiguration>
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly IHttpClientFactory _httpClientFactory;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public ConsulServiceRequestHandler(IServiceResolver serviceResolver, IHttpClientFactory httpClientFactory)
        {
            _serviceResolver = serviceResolver;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, ConsulServiceRouterConfiguration routerConfiguration, CancellationToken cancellationToken)
        {
            var endpoint = await _serviceResolver.GetRandomEndpoint(routerConfiguration.Service, routerConfiguration.Tag, cancellationToken);
            if (endpoint == null)
            {
                Logger.Warn($"Can't find service: {routerConfiguration.Service}/{routerConfiguration.Tag}");
                return requestMessage.CreateResponse(HttpStatusCode.NotFound);
            }
            var uri = CreateTargetUri(requestMessage.RequestUri, routerConfiguration, endpoint);
            requestMessage = PrepareRequestForForwarding(requestMessage, uri, routerConfiguration);
            var client = _httpClientFactory.GetHttpClient();
            try
            {
                return await client.SendAsync(requestMessage, cancellationToken);
            }
            finally
            {
                _httpClientFactory.Release(client);
            }
        }
    }
}