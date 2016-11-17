using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;

namespace ApiRouter.Core
{
    public class Configuration : IConfiguration
    {
        private readonly IConsulClient _consulClient;

        public Configuration(IConsulClient consulClient)
        {
            _consulClient = consulClient;
        }

        public async Task<string> GetClusterName(CancellationToken cancellationToken = new CancellationToken())
        {
            var nodeName = await GetAgentName(cancellationToken);
            var result = await _consulClient.KV.Get($"requestrouter/servers/{nodeName}/cluster".ToLower(), cancellationToken);
            var clusterName = null as string;
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                result = await _consulClient.KV.Get("requestrouter/defaults/cluster", cancellationToken);
            }

            if (result?.Response?.Value != null)
            {
                clusterName = Encoding.UTF8.GetString(result.Response.Value);
            }

            return clusterName;
        }

        public async Task<string> GetAgentName(CancellationToken cancellationToken = new CancellationToken())
        {
            return (await _consulClient.Agent.GetNodeName(cancellationToken)) ?? Environment.MachineName;
        }

        private async Task<string> GetStringConsulSetting(string path, CancellationToken cancellationToken)
        {
            var response = await _consulClient.KV.Get($"requestrouter/{path}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var bytes = response.Response?.Value;
                if (bytes != null)
                {
                    return Encoding.UTF8.GetString(bytes);
                }
            }
            return null;
        }

        public async Task<bool> GetCacheEnabled(CancellationToken cancellationToken = new CancellationToken())
        {
            bool canCache;
            var clusterName = await GetClusterName(cancellationToken);
            var agentName = await GetAgentName(cancellationToken);
            string settingString = null;
            if (!string.IsNullOrWhiteSpace(clusterName))
            {
                settingString = await GetStringConsulSetting($"cluster/{clusterName}/config/cache/{agentName}".ToLower(), cancellationToken);
                if (bool.TryParse(settingString, out canCache))
                    return canCache;
                settingString = await GetStringConsulSetting($"cluster/{clusterName}/config/cache".ToLower(), cancellationToken);
                if (bool.TryParse(settingString, out canCache))
                    return canCache;
            }
            settingString = await GetStringConsulSetting($"defaults/config/cache/{agentName}".ToLower(), cancellationToken);
            if (bool.TryParse(settingString, out canCache))
                return canCache;
            settingString = await GetStringConsulSetting("defaults/config/cache", cancellationToken);
            if (bool.TryParse(settingString, out canCache))
                return canCache;
            return false;
        }
    }
}