using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [DirectiveName("host")]
    public class HostConfiguration : ConfigurationEntry
    {
        [JsonProperty("hosts")]
        public string[] Hosts { get; set; }

        public override Task<bool> IsMatch(HttpRequestMessage request)
        {
            var localHosts = Hosts.ToList();
            foreach (var Host in localHosts)
            {
                var currentHost = request.Headers.Host.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(currentHost)) continue;
                if (Host.StartsWith("*")
                    ? currentHost.EndsWith(Host.Substring(1), StringComparison.InvariantCultureIgnoreCase)
                    : currentHost.Equals(Host, StringComparison.InvariantCultureIgnoreCase))
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}