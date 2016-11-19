using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core
{
    [Named("consulService")]
    public class ConsulServiceRouterConfiguration : RequestRouterConfigurationBase
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
    }
}