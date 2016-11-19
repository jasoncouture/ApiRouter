using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;

namespace ApiRouter.Core.Handlers
{
    public abstract class RequestHandlerModuleBase<TRequest> : IRequestHandlerModule where TRequest : RequestRouterConfigurationBase
    {
        public Task<bool> ShouldProcessRequest(HttpRequestMessage requestMessage, RequestRouterConfigurationBase configuration, CancellationToken cancellationToken)
        {
            var config = configuration as TRequest;
            if (config == null) return Task.FromResult(false);
            return ShouldProcessRequest(requestMessage, config, cancellationToken);
        }

        protected virtual Task<bool> ShouldProcessRequest(HttpRequestMessage requestMessage, TRequest configuration, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        protected Uri CreateTargetUri(Uri sourceUri, RequestRouterConfigurationBase config, DnsEndPoint endPoint)
        {
            var builder = new UriBuilder(sourceUri)
            {
                Port = config.Port ?? endPoint.Port,
                Host = endPoint.Host,
            };

            if (!string.IsNullOrWhiteSpace(config.Scheme))
                builder.Scheme = config.Scheme;

            

            return builder.Uri;
        }

        protected virtual HttpRequestMessage PrepareRequestForForwarding(HttpRequestMessage request, Uri realTarget, RequestRouterConfigurationBase configuration, string configHost = null)
        {
            foreach (var header in configuration.Headers ?? new Dictionary<string, string>())
            {
                if (request.Headers.Any(x => string.Equals(x.Key, header.Key, StringComparison.InvariantCultureIgnoreCase))) continue;
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            request.Headers.Add("X-Forwarded-For", request.GetOwinContext().Request.RemoteIpAddress);
            request.Headers.Add("X-Original-Uri", request.RequestUri.ToString());
            request.Headers.Add("X-Forwarded-Proto", request.RequestUri.Scheme);
            request.Headers.Add("X-Forwarded-Host", request.Headers.Host);
            string host = null;
            if (!configuration.Headers?.TryGetValue("Host", out host) ?? false)
                host = null;
            request.Headers.Host = host ?? configHost ?? (realTarget.IsDefaultPort ? realTarget.Host : $"{realTarget.Host}:{realTarget.Port}");
            request.RequestUri = realTarget;
            return request;
        }


        public Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, RequestRouterConfigurationBase configuration, CancellationToken cancellationToken)
        {
            return ProcessRequest(requestMessage, configuration as TRequest, cancellationToken);
        }

        protected abstract Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage,
            TRequest configuration, CancellationToken cancellationToken);
    }

    public class ConsulServiceRequestHandler : RequestHandlerModuleBase<ConsulServiceRouterConfiguration>
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly IHttpClientFactory _httpClientFactory;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public ConsulServiceRequestHandler(IServiceResolver serviceResolver, IHttpClientFactory httpClientFactory)
        {
            _serviceResolver = serviceResolver;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, ConsulServiceRouterConfiguration routerConfiguration, CancellationToken cancellationToken)
        {
            var endpoint = await _serviceResolver.GetRandomEndpoint(routerConfiguration.Service, routerConfiguration.Tag, cancellationToken);
            if (endpoint == null)
            {
                Logger.Warn($"Can't find service: {routerConfiguration.Service}/{routerConfiguration.Tag}");
                return requestMessage.CreateResponse(HttpStatusCode.NotFound);
            }
            var uri = CreateTargetUri(requestMessage.RequestUri, routerConfiguration, endpoint);
            requestMessage = PrepareRequestForForwarding(requestMessage, uri, routerConfiguration);
            var client = _httpClientFactory.GetHttpClient();
            try
            {
                return await client.SendAsync(requestMessage, cancellationToken);
            }
            finally
            {
                _httpClientFactory.Release(client);
            }
        }
    }

    public class ResponseCodeRequestHandler : RequestHandlerModuleBase<ResponseCodeRouterConfiguration>
    {
        protected override Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, ResponseCodeRouterConfiguration configuration,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(requestMessage.CreateResponse(configuration.StatusCode));
        }
    }

    public class HostRequestHandler : RequestHandlerModuleBase<HostRouterConfiguration>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HostRequestHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage, HostRouterConfiguration routerConfiguration,
            CancellationToken cancellationToken)
        {
            var endpoint = new DnsEndPoint(routerConfiguration.Host, routerConfiguration.Port ?? requestMessage.RequestUri.Port);
            var uri = CreateTargetUri(requestMessage.RequestUri, routerConfiguration, endpoint);
            requestMessage = PrepareRequestForForwarding(requestMessage, uri, routerConfiguration);
            var client = _httpClientFactory.GetHttpClient();
            try
            {
                return await client.SendAsync(requestMessage, cancellationToken);
            }
            catch
            {
                return requestMessage.CreateResponse(HttpStatusCode.BadGateway);
            }
            finally
            {
                _httpClientFactory.Release(client);
            }
        }
    }
}
