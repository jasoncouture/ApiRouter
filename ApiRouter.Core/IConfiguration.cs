using System.Threading;
using System.Threading.Tasks;

namespace ApiRouter.Core
{
    public interface IConfiguration
    {
        Task<string> GetClusterName(CancellationToken cancellationToken = default(CancellationToken));
        Task<string> GetAgentName(CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> GetCacheEnabled(CancellationToken cancellationToken = default(CancellationToken));
    }
}