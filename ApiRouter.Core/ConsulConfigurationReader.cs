using System;
using System.ComponentModel.Design;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;
using Consul;
using Newtonsoft.Json;

namespace ApiRouter.Core
{
    public class ConsulConfigurationReader : IConfigurationReader
    {
        private readonly IConsulClient _conulClient;
        private readonly JsonSerializer _serializer;
        public ILogger Logger { get; set; } = NullLogger.Instance;
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


            QueryResult<KVPair> configResult = null;
            configResult = await _conulClient.KV.Get(configPathBuilder.ToString().ToLower(), cancellationToken);
            if (configResult.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"Configuration not found at config path: {configPathBuilder}");
            var modificationToken = configResult.Response.ModifyIndex;
            if (_modificationToken == modificationToken) return _configuration;
            lock (_lockObject)
            {
                configResult = _conulClient.KV.Get(configPathBuilder.ToString().ToLower(), cancellationToken).GetAwaiter().GetResult();
                if (configResult.StatusCode != HttpStatusCode.OK)
                    throw new InvalidOperationException(
                        $"Configuration not found at config path: {configPathBuilder}");
                modificationToken = configResult.Response.ModifyIndex;
                if (_modificationToken == modificationToken) return _configuration;
                using (var memoryStream = new MemoryStream(configResult.Response.Value))
                using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    Logger.Info(
                        _modificationToken != 0
                            ? $"Configuration seems to have changed, modification token: {_modificationToken} does not match {modificationToken}, parsing new config."
                            : $"Reading configuration, Modification token: {modificationToken}");
                    var config = new RouterConfiguration();
                    config.Config.AddRange(_serializer.Deserialize<ConfigContainer[]>(jsonReader));
                    _configuration = config;
                    _modificationToken = modificationToken;
                    Logger.Info(
                        $"Parsed new config with {config.Config.Count} block{(config.Config.Count == 1 ? "" : "s")}");
                    return config;
                }
            }
        }

        private readonly object _lockObject = new object();
        private ulong _modificationToken = 0;
        private volatile RouterConfiguration _configuration = null;
    }
}