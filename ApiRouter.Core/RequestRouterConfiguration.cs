using Newtonsoft.Json;

namespace ApiRouter.Core
{
    public class RequestRouterConfiguration
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
        [JsonProperty("scheme")]
        public string Scheme { get; set; }
        [JsonProperty("port")]
        public int? Port { get; set; }
        public string Host { get; set; }
    }
}