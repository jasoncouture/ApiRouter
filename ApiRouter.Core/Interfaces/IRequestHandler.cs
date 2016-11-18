using System.Net.Http;
using System.Threading.Tasks;

namespace ApiRouter.Core.Interfaces
{
    public interface IRequestHandler
    {
        Task<HttpResponseMessage> HandleRequest(HttpRequestMessage request);
    }
}