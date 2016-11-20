using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [Named("ports")]
    public class PortsConfiguration : ConfigurationEntry
    {
        [JsonProperty("ports")]
        public int[] Ports { get; set; }
        public override Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Ports?.Contains(request.GetOwinContext().Request.LocalPort ?? -1) ?? false);
        }
    }
}