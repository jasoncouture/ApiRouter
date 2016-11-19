using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;

namespace ApiRouter.Core
{
    public class ProxyHandler : DelegatingHandler
    {
        private readonly IRequestHandler _requestHandler;
        private readonly IConfigurationReader _configurationReader;
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public ProxyHandler(IRequestHandler requestHandler, IConfigurationReader configurationReader)
        {
            _requestHandler = requestHandler;
            _configurationReader = configurationReader;
        }

        public async Task<RequestRouterConfigurationBase> GetConfigurationForRequest(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var configuration = await _configurationReader.GetConfiguration(cancellationToken);
            var localConfigs = new List<ConfigContainer>((configuration.Config ?? new List<ConfigContainer>()).Where(i => i.Rule != null && i.Route != null));
            foreach (var config in localConfigs)
            {
                try
                {
                    if (await config.IsMatch(requestMessage, cancellationToken))
                        return config.Route;
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to handle a rule due to an exception, Bad configuration?", ex);
                }
            }

            var route = localConfigs.FirstOrDefault(i => i.Default)?.Route;
            if (route == null)
            {
                Logger.Error($"No configuration found!");
                throw new NoConfigurationMatchException();
            }
            return route;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var finalConfig = await GetConfigurationForRequest(request, cancellationToken);
            var response = await _requestHandler.HandleRequest(request, finalConfig, cancellationToken);
            request.RegisterForDispose(response);
            return response;
        }
    }
}