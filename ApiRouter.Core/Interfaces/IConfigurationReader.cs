using System.IO;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;

namespace ApiRouter.Core.Interfaces
{
    public interface IConfigurationReader
    {
        Task<RouterConfiguration> ParseRouterConfiguration(Stream stream);
    }
}