using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;
using Castle.Windsor;

namespace ApiRouter.Core
{
    public class ProxyHandler : DelegatingHandler
    {
        private readonly IWindsorContainer _container;
        private readonly IServiceParser _serviceParser;
        private readonly IWeakCache _weakCache;
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public ProxyHandler(IWindsorContainer container, IServiceParser serviceParser, IWeakCache weakCache)
        {
            _container = container;
            _serviceParser = serviceParser;
            _weakCache = weakCache;
        }

        private Uri CreateTargetUri(Uri sourceUri, RequestRouterConfigurationBase config, DnsEndPoint endPoint)
        {
            var builder = new UriBuilder(sourceUri)
            {
                Port = endPoint.Port,
                Host = endPoint.Host,
            };

            if (!string.IsNullOrWhiteSpace(config.Scheme))
                builder.Scheme = config.Scheme;

            return builder.Uri;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var serviceResolver = _container.Resolve<IServiceResolver>();
            var service = await _serviceParser.GetServiceAsync(request, cancellationToken);
            var originalUri = request.RequestUri;
            DateTimeOffset start = DateTimeOffset.Now;
            HttpResponseMessage responseMessage = request.CreateResponse(HttpStatusCode.InternalServerError);
            try
            {
                if (service == null)
                {
                    Logger.Warn($"No configuration could be found for this URI: {request.RequestUri}");
                    return responseMessage = request.CreateResponse(HttpStatusCode.NotFound, "Not Found");
                }
                var endpoint = await serviceResolver.GetRandomEndpoint(service.Service, service.Tag, cancellationToken);
                if (endpoint == null)
                {
                    Logger.Warn($"No backend server is known for: {request.RequestUri}");
                    return responseMessage = request.CreateResponse(HttpStatusCode.NotFound);
                }
                var forwardUri = CreateTargetUri(request.RequestUri, service, endpoint);


                //strip off the proxy port and replace with an Http port
                //send it on to the requested URL

                request.RequestUri = forwardUri;
                var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = false
                })
                {
                    DefaultRequestHeaders = { Connection = { "close" } },
                    Timeout = TimeSpan.FromHours(2)
                };
                request.RegisterForDispose(client);

                if (request.Method == HttpMethod.Get)
                {
                    request.Content = null;
                }

                request.Headers.Add("X-Forwarded-For", request.GetOwinContext().Request.RemoteIpAddress);
                request.Headers.Add("X-Original-Uri", originalUri.ToString());
                request.Headers.Add("X-Forwarded-Proto", originalUri.Scheme);
                request.Headers.Add("X-Forwarded-Host", request.Headers.Host);
                request.Headers.Host = service.HostHeader ?? service.Host ?? (request.RequestUri.IsDefaultPort ? request.RequestUri.Host : $"{request.RequestUri.Host}:{request.RequestUri.Port}");
                responseMessage = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                return responseMessage;
            }
            catch (Exception ex)
            {
                Logger.Warn("An exception occured while attempting to forward a request to another service.", ex);
                return responseMessage = request.CreateResponse(HttpStatusCode.BadGateway, "Bad Gateway");
            }
            finally
            {
                request.RegisterForDispose(responseMessage);
                var taken = DateTimeOffset.Now - start;
                Logger.Info($"{request.Method.Method} {originalUri} -> {request.Method.Method} {request.RequestUri} - {responseMessage.StatusCode} ({taken})");
                _container.Release(serviceResolver);
            }
        }
    }
}