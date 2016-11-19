using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [Named("hosts")]
    public class HostsConfiguration : ConfigurationEntry
    {
        [JsonProperty("hosts")]
        public string[] Hosts { get; set; }

        public override Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentHost = request.RequestUri.Host;
            if (string.IsNullOrEmpty(currentHost)) return Task.FromResult(false);
            var localHosts = Hosts.ToList();
            foreach (var host in localHosts)
            {
                if (host.StartsWith("*")
                    ? currentHost.EndsWith(host.Substring(1), StringComparison.InvariantCultureIgnoreCase)
                    : currentHost.Equals(host, StringComparison.InvariantCultureIgnoreCase))
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}