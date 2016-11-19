using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Interfaces;

namespace ApiRouter.Core.Handlers
{
    public class LegacyRequestHandler : RequestHandlerModuleBase<RequestRouterConfiguration>
    {
        private readonly IRequestHandlerFactory _requestHandlerFactory;

        public LegacyRequestHandler(IRequestHandlerFactory requestHandlerFactory)
        {
            _requestHandlerFactory = requestHandlerFactory;
        }

        protected override Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, RequestRouterConfiguration configuration,
            CancellationToken cancellationToken)
        {
            RequestRouterConfigurationBase realConfiguration = string.IsNullOrWhiteSpace(configuration.Service) ? (RequestRouterConfigurationBase)((HostRouterConfiguration)configuration) : (RequestRouterConfigurationBase)((ConsulServiceRouterConfiguration)configuration);
            var handler = _requestHandlerFactory.GetHandler();
            try
            {
                return handler.HandleRequest(requestMessage, configuration, cancellationToken);
            }
            finally
            {
                _requestHandlerFactory.Release(handler);
            }
        }
    }
}