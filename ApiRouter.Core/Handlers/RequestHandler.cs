using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;

namespace ApiRouter.Core.Handlers
{
    public class RequestHandler : IRequestHandler
    {
        private readonly List<IRequestHandlerModule> _modules;
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public RequestHandler(IEnumerable<IRequestHandlerModule> modules)
        {
            _modules = modules.ToList();
        }
        public async Task<HttpResponseMessage> HandleRequest(HttpRequestMessage request, RequestRouterConfigurationBase configuration,
            CancellationToken cancellationToken)
        {
            var localModules = new List<IRequestHandlerModule>(_modules);
            var response = request.CreateResponse(HttpStatusCode.BadGateway);
            var originalUri = request.RequestUri;
            try
            {
                foreach (var module in localModules)
                {
                    if (await module.ShouldProcessRequest(request, configuration, cancellationToken))
                    {
                        response = await module.ProcessRequest(request, configuration, cancellationToken);
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Warn("An exception occured processing a request.", ex);
            }
            Logger.Info($"{request.GetOwinContext().Request.RemoteIpAddress} {request.Method} {originalUri} - {response.StatusCode}");
            return response;
        }
    }
}