using System.Net.Http;

namespace ApiRouter.Core.Interfaces
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
        void Release(HttpClient client);
    }
}