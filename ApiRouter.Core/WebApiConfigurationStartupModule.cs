using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using ApiRouter.Core.Config.JsonConverters;
using Castle.Core.Logging;
using Castle.Windsor;
using Newtonsoft.Json;

namespace ApiRouter.Core
{
    public class WebApiConfigurationStartupModule
    {
        private readonly HttpConfiguration _configuration;
        private readonly ProxyHandler _proxyHandler;
        private readonly JsonSerializerSettings _settings;
        private readonly List<JsonConverter> _converters;
        public ILogger Logger { get; set; }
        public WebApiConfigurationStartupModule(HttpConfiguration configuration, ProxyHandler proxyHandler, JsonSerializerSettings settings, IEnumerable<JsonConverter> converters)
        {
            _configuration = configuration;
            _proxyHandler = proxyHandler;
            _settings = settings;
            _converters = converters.ToList();
        }

        public void Start()
        {
            foreach (var item in _converters)
            {
                if (_settings.Converters.Any(i => i.GetType() == item.GetType())) continue;
                _settings.Converters.Add(item);
            }
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