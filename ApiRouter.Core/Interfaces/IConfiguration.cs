using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;

namespace ApiRouter.Core.Interfaces
{
    public interface IConfiguration
    {
        Task<string> GetClusterName(CancellationToken cancellationToken = default(CancellationToken));
        Task<string> GetAgentName(CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> GetCacheEnabled(CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
        void Release(HttpClient client);
    }

    public interface IConfigurationReader
    {
        Task<RouterConfiguration> ParseRouterConfiguration(Stream stream);
    }


}