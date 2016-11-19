using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;

namespace ApiRouter.Core.Interfaces
{
    public interface IConfigurationReader
    {
        Task<RouterConfiguration> GetConfiguration(CancellationToken cancellationToken);
    }
}