using System.Linq;
using ApiRouter.Core.Config.Attributes;
using ApiRouter.Core.Config.Models;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;

namespace ApiRouter.Core
{
    [Named("__default")]
    public class RequestRouterConfiguration : RequestRouterConfigurationBase
    {
        private string _tag;
        [JsonProperty("service")]
        public string Service { get; set; }
        [JsonProperty("tag")]
        public string Tag
        {
            get { return _tag; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                _tag = value;
            }
        }
        [JsonProperty("hostHeader")]
        public string HostHeader { get; set; }
        [JsonProperty("host")]
        public string Host { get; set; }

        public static implicit operator HostRouterConfiguration(RequestRouterConfiguration config)
        {
            return new HostRouterConfiguration
            {
                Headers = config.Headers.ToDictionary(k => k.Key, v => v.Value),
                Host = config.Host,
                Port = config.Port,
                Scheme = config.Scheme
            };
        }

        public static implicit operator ConsulServiceRouterConfiguration(RequestRouterConfiguration config)
        {
            return new ConsulServiceRouterConfiguration
            {
                Headers = config.Headers.ToDictionary(k => k.Key, v => v.Value),
                Service = config.Service,
                Tag = config.Tag,
                Port = config.Port,
                Scheme = config.Scheme
            };
        }
    }
}