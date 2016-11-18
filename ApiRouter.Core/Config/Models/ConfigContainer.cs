using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    public sealed class ConfigContainer
    {
        [JsonProperty("rule")]
        public ConfigurationEntry Rule { get; set; }
        [JsonProperty("route")]
        public RequestRouterConfiguration Route { get; set; }
        [JsonProperty("default")]
        public bool Default { get; set; } = false;
        public async Task<bool> IsMatch(HttpRequestMessage request)
        {
            var task = Rule?.IsMatch(request);
            if (task == null) return false;
            return await task;
        }
    }
}