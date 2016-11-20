using System.Collections.Generic;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;

namespace ApiRouter.Core.Interfaces
{
    public interface INodeConfigurationProvider
    {
        Task<IEnumerable<NodeConfiguration>> GetConfigurationAsync(string nodeName = null);
    }
}