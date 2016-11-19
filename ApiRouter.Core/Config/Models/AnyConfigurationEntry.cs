using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;

namespace ApiRouter.Core.Config.Models
{
    [Named("any")]
    public sealed class AnyConfigurationEntry : ConfigurationEntry
    {
        public List<ConfigurationEntry> Children { get; set; } = new List<ConfigurationEntry>();
        public override async Task<bool> IsMatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var localChildren = new List<ConfigurationEntry>(Children);
            foreach (var entry in localChildren)
            {
                if (await entry.IsMatch(request, cancellationToken))
                    return true;
            }
            return false;
        }
    }
}