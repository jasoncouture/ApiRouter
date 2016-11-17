using System.Net.Http;
using System.Web.Http;
using Castle.Core.Logging;

namespace ApiRouter.Core
{
    public class WebApiConfigurationStartupModule
    {
        private readonly HttpConfiguration _configuration;
        private readonly ProxyHandler _proxyHandler;
        public ILogger Logger { get; set; }
        public WebApiConfigurationStartupModule(HttpConfiguration configuration, ProxyHandler proxyHandler)
        {
            _configuration = configuration;
            _proxyHandler = proxyHandler;
        }

        public void Start()
        {
            Logger.Info($"Configuring Web API");
            _configuration.Routes.MapHttpRoute(name: "Proxy", routeTemplate: "{*path}",
                handler: HttpClientFactory.CreatePipeline
                    (
                        innerHandler: new HttpClientHandler(), // will never get here if proxy is doing its job
                        handlers: new DelegatingHandler[] { _proxyHandler }
                    ),
                defaults: new { path = RouteParameter.Optional },
                constraints: null
                );
        }
    }
}