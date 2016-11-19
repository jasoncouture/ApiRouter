using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRouter.Core.Interfaces
{
    public interface IServiceResolver
    {
        Task<DnsEndPoint> GetRandomEndpoint(string service, string tag, CancellationToken cancellationToken);
        Task<IEnumerable<DnsEndPoint>> GetAllEndpoints(string service, string tag, CancellationToken cancellationToken);
    }
}