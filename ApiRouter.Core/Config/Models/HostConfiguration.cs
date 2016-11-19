using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [Named("host")]
    public class HostConfiguration : ConfigurationEntry
    {
        [JsonProperty("host")]
        public string Host { get; set; }
        public override Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentHost = request.RequestUri.Host;
            if (string.IsNullOrWhiteSpace(currentHost)) return Task.FromResult(false);
            return Task.FromResult(Host.StartsWith("*")
                ? currentHost.EndsWith(Host.Substring(1), StringComparison.InvariantCulture)
                : currentHost.Equals(Host, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}