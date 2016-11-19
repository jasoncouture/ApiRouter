using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [Named("pathPrefix")]
    public class PathPrefixConfiguration : ConfigurationEntry
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; } = "/";

        public override Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.RequestUri.AbsolutePath.StartsWith(Prefix ?? "/", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}