using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;
using Consul;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiRouter.Core
{
    public class NodeConfigurationProvider : INodeConfigurationProvider
    {
        private readonly IConsulClient _consulClient;
        private readonly JsonSerializer _serializer;
        public ILogger Logger { get; set; }
        public NodeConfigurationProvider(IConsulClient consulClient, JsonSerializer serializer)
        {
            _consulClient = consulClient;
            _serializer = serializer;
        }

        public async Task<IEnumerable<NodeConfiguration>> GetConfigurationAsync(string nodeName = null)
        {
            nodeName = nodeName ?? Environment.MachineName;
            nodeName = nodeName.ToLower();
            Logger.Info($"Loading node specific configuration for node: {nodeName}");
            var response = await _consulClient.KV.Get($"requestrouter/servers/{nodeName}/config".ToLower());
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.Info("No node configuration found, looking up defaults");
                response = await _consulClient.KV.Get($"requestrouter/defaults/server/config");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.Warn("No configuration was found in consul, using system defaults.");
                return new[] { NodeConfiguration.Default };
            }

            using (var memoryStream = new MemoryStream(response.Response.Value))
            using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var token = JToken.Load(jsonTextReader);
                if (token.Type == JTokenType.Object)
                {
                    return new[] { token.ToObject<NodeConfiguration>(_serializer) };
                }
                else if (token.Type == JTokenType.Array)
                {
                    return token.ToObject<IEnumerable<NodeConfiguration>>(_serializer);
                }
                else
                {
                    throw new InvalidOperationException($"The configuration provided is not valid. The configuration must either be an object or array of settings.");
                }
            }
        }
    }
}