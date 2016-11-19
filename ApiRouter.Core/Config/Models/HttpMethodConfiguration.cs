using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [Named("httpMethod")]
    public class HttpMethodConfiguration : ConfigurationEntry
    {
        [JsonProperty("methods")]
        public string[] Methods { get; set; }

        public override Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return
                Task.FromResult(
                    Methods?.Any(
                        x => string.Equals(x, request.Method.Method, StringComparison.InvariantCultureIgnoreCase)) ??
                    false);
        }
    }
}