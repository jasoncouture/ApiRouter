using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [Named("pathRegex")]
    public class PathRegularExpressionConfiguration : ConfigurationEntry
    {
        [JsonProperty("pathRegex")]
        public Regex Regex { get; set; }
        public override Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Regex.IsMatch(request.RequestUri.AbsolutePath));
        }
    }
}