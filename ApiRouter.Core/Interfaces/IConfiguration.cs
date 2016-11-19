using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRouter.Core.Interfaces
{
    public interface IConfiguration
    {
        Task<string> GetClusterName(CancellationToken cancellationToken = default(CancellationToken));
        Task<string> GetAgentName(CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> GetCacheEnabled(CancellationToken cancellationToken = default(CancellationToken));
    }
}