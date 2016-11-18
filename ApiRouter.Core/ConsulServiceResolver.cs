using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Interfaces;
using Castle.Components.DictionaryAdapter;
using Castle.Core.Logging;
using Consul;

namespace ApiRouter.Core
{
    public class ConsulServiceResolver : IServiceResolver
    {
        private readonly IConsulClient _consulClient;
        private readonly IWeakCache _weakCache;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public ConsulServiceResolver(IConsulClient consulClient, IWeakCache weakCache)
        {
            _consulClient = consulClient;
            _weakCache = weakCache;
        }

        public async Task<DnsEndPoint> GetRandomEndpoint(RequestRouterConfiguration serviceConfig,
            CancellationToken cancellationToken)
        {
            var allEndpoints = await GetAllEndpoints(serviceConfig, cancellationToken);
            var selectedEndpoint = allEndpoints.OrderBy(i => Guid.NewGuid()).FirstOrDefault();
            return selectedEndpoint;
        }

        public async Task<IEnumerable<DnsEndPoint>> GetAllEndpoints(RequestRouterConfiguration serviceConfig,
            CancellationToken cancellationToken)
        {

            List<DnsEndPoint> results = new EditableList<DnsEndPoint>();
            if (!string.IsNullOrWhiteSpace(serviceConfig.Host))
            {
                var port = serviceConfig.Port ?? (serviceConfig.Scheme == "https" ? 443 : 80);
                results.Add(new DnsEndPoint(serviceConfig.Host, port));
            }

            if (!string.IsNullOrWhiteSpace(serviceConfig.Service))
            {
                var serviceEndpoints =
                    await
                        _consulClient.Health.Service(serviceConfig.Service, serviceConfig.Tag, cancellationToken);
                if (serviceEndpoints.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.Warn($"Couldn't find service: {serviceConfig.Service}, with tag {serviceConfig.Tag}");
                }
                else if (serviceEndpoints.StatusCode == HttpStatusCode.OK)
                {
                    var endpoints = serviceEndpoints.Response
                        .Where(i => !i.Checks.Any() || i.Checks.All(x => string.Equals(x.Status, "passing", StringComparison.InvariantCultureIgnoreCase) || string.Equals(x.Status, "warning", StringComparison.InvariantCultureIgnoreCase)))
                        .Select(i => new DnsEndPoint(string.IsNullOrWhiteSpace(i.Service.Address) ? i.Service.Address : i.Node.Address, i.Service.Port == 0 ? 80 : i.Service.Port))
                        .ToList();
                    results.AddRange(endpoints);
                }
            }
            return results;
        }
    }
}