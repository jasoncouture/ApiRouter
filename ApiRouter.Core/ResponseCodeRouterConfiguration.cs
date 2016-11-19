using System.Net;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core
{
    [Named("statusCode")]
    public class ResponseCodeRouterConfiguration : RequestRouterConfigurationBase
    {
        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; }
    }
}