using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    public sealed class ConfigContainer
    {
        [JsonProperty("rule")]
        public ConfigurationEntry Rule { get; set; }
        [JsonProperty("route")]
        public RequestRouterConfigurationBase Route { get; set; }
        [JsonProperty("default")]
        public bool Default { get; set; } = false;
        public async Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var task = Rule?.IsMatch(request, cancellationToken);
            if (task == null) return false;
            return await task;
        }
    }
}