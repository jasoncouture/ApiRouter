using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [Named("port")]
    public class PortConfiguration : ConfigurationEntry
    {
        [JsonProperty("port")]
        public int Port { get; set; }
        public override Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.GetOwinContext().Request.LocalPort == Port);
        }
    }
}