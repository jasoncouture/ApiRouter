using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;
using ApiRouter.Core.Interfaces;
using Consul;
using Newtonsoft.Json;

namespace ApiRouter.Core
{
    public class ConsulConfigurationReader : IConfigurationReader
    {
        private readonly IConsulClient _conulClient;
        private readonly JsonSerializer _serializer;

        public ConsulConfigurationReader(IConsulClient conulClient, JsonSerializer serializer)
        {
            _conulClient = conulClient;
            _serializer = serializer;
        }

        public async Task<RouterConfiguration> GetConfiguration(CancellationToken cancellationToken)
        {
            string clusterName = null;
            var result = await _conulClient.KV.Get($"requestrouter/servers/{await _conulClient.Agent.GetNodeName(cancellationToken)}/cluster".ToLower(), cancellationToken);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                clusterName = Encoding.UTF8.GetString(result.Response.Value);
            }

            var configPathBuilder = new StringBuilder("requestrouter/");
            if (!string.IsNullOrWhiteSpace(clusterName))
            {
                configPathBuilder.Append($"cluster/{clusterName}/");
            }

            configPathBuilder.Append("config");

            var configResult = await _conulClient.KV.Get(configPathBuilder.ToString().ToLower(), cancellationToken);
            if (configResult.StatusCode != HttpStatusCode.OK) throw new InvalidOperationException($"Configuration not found at config path: {configPathBuilder}");
            var modificationToken = configResult.Response.ModifyIndex;
            if (_modificationToken == modificationToken) return _configuration;
            // The reason we don't do any locking here is Stale data is okay.
            // Configuration during changes on the server may reuslt in inconsistencies, but this is expected in a distributed system.

            using (var memoryStream = new MemoryStream(configResult.Response.Value))
            using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var config = _serializer.Deserialize<RouterConfiguration>(jsonReader);
                _configuration = config;
                _modificationToken = modificationToken;
                return config;
            }
        }
        private ulong _modificationToken = 0;
        private volatile RouterConfiguration _configuration = null;
    }
}