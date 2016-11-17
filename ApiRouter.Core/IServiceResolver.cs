using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRouter.Core
{
    public interface IServiceResolver
    {
        Task<DnsEndPoint> GetRandomEndpoint(RequestRouterConfiguration serviceConfig, CancellationToken cancellationToken);
        Task<IEnumerable<DnsEndPoint>> GetAllEndpoints(RequestRouterConfiguration serviceConfig, CancellationToken cancellationToken);
    }
}