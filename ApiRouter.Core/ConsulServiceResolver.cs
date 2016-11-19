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

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public ConsulServiceResolver(IConsulClient consulClient)
        {
            _consulClient = consulClient;
        }

        public async Task<DnsEndPoint> GetRandomEndpoint(string service, string tag,
            CancellationToken cancellationToken)
        {
            var allEndpoints = await GetAllEndpoints(service, tag, cancellationToken);
            var selectedEndpoint = allEndpoints.OrderBy(i => Guid.NewGuid()).FirstOrDefault();
            return selectedEndpoint;
        }

        public async Task<IEnumerable<DnsEndPoint>> GetAllEndpoints(string service, string tag,
            CancellationToken cancellationToken)
        {

            List<DnsEndPoint> results = new EditableList<DnsEndPoint>();

            var serviceEndpoints = await _consulClient.Health.Service(service, tag, cancellationToken);
            if (serviceEndpoints.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.Warn($"Couldn't find service: {service}/{tag}");
            }
            else if (serviceEndpoints.StatusCode == HttpStatusCode.OK)
            {
                var endpoints = serviceEndpoints.Response
                    .Where(i => !i.Checks.Any() || i.Checks.All(x => string.Equals(x.Status, "passing", StringComparison.InvariantCultureIgnoreCase) || string.Equals(x.Status, "warning", StringComparison.InvariantCultureIgnoreCase)))
                    .Select(i => new DnsEndPoint(string.IsNullOrWhiteSpace(i.Service.Address) ? i.Service.Address : i.Node.Address, i.Service.Port == 0 ? 80 : i.Service.Port))
                    .ToList();
                results.AddRange(endpoints);
            }

            return results;
        }
    }
}