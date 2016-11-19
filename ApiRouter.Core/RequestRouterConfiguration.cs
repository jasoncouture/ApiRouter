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
    }
}