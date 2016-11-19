using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;

namespace ApiRouter.Core.Config.Models
{
    [Named("all")]
    public sealed class AllConfigurationEntry : ConfigurationEntry
    {
        public List<ConfigurationEntry> Children { get; set; } = new List<ConfigurationEntry>();
        public override async Task<bool> IsMatch(HttpRequestMessage request)
        {
            var localChildren = new List<ConfigurationEntry>(Children);
            if (localChildren.Count == 0) return false;
            foreach (var entry in localChildren)
            {
                if (!await entry.IsMatch(request))
                    return false;
            }
            return true;
        }
    }
}