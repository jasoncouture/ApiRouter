using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Models;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;
using Consul;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiRouter.Core
{
    public class ServiceParser : IServiceParser
    {
        private readonly IConsulClient _consulClient;
        private readonly JsonSerializer _serializer;
        private readonly IWeakCache _weakCache;
        private readonly IConfiguration _configuration;
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public ServiceParser(IConsulClient consulClient, JsonSerializer serializer, IWeakCache weakCache, IConfiguration configuration)
        {
            _consulClient = consulClient;
            _serializer = serializer;
            _weakCache = weakCache;
            _configuration = configuration;
        }

        private RequestRouterConfiguration TryParse(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json)) return null;
                var token = JToken.Parse(json);
                return token.ToObject<RequestRouterConfiguration>(_serializer);
            }
            catch
            {
                return null;
            }
        }

        private async Task<RequestRouterConfiguration> GetRequestSettingFromPath(string path, CancellationToken cancellationToken)
        {
            var clusterName = await _configuration.GetClusterName(cancellationToken);
            var fullPath = string.IsNullOrWhiteSpace(clusterName) ? $"requestrouter/defaults/config/{path}" : $"requestrouter/cluster/{clusterName}/config/{path}";
            fullPath = fullPath.ToLower();
            Logger.Debug($"Reading setting: {fullPath}");
            var settingsQueryResult = await _consulClient.KV.Get(fullPath, cancellationToken);
            RequestRouterConfiguration ret = null;
            ret = settingsQueryResult.StatusCode == HttpStatusCode.NotFound ? null : TryParse(Encoding.UTF8.GetString(settingsQueryResult.Response.Value));
            Logger.Debug(ret != null
                ? $"Valid configuration found at: {fullPath}"
                : $"Could not find configuration at: {fullPath}");
            return ret;
        }

        private string GetConfigUrl(string host = null, string method = null, string path = null)
        {
            StringBuilder ret = new StringBuilder("services");
            if (!string.IsNullOrWhiteSpace(host))
            {
                ret.Append($"/host/{host.ToLower()}");
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                ret.Append($"/method/{method.ToLower()}");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                ret.Append("/root");
            }
            else
            {
                ret.Append($"/url/{path.ToLower()}");
            }

            return ret.ToString();
        }

        public async Task<RequestRouterConfiguration> GetConfigurationForRequest(RouterConfiguration configuration, HttpRequestMessage requestMessage)
        {
            var localConfigs = new List<ConfigContainer>((configuration.Config ?? new List<ConfigContainer>()).Where(i => i.Rule != null && i.Route != null));
            foreach (var config in localConfigs)
            {
                try
                {
                    if (await config.IsMatch(requestMessage))
                        return config.Route;
                }
                catch(Exception ex)
                {
                    Logger.Warn($"Failed to handle a rule due to an exception, Bad configuration?", ex);
                }
            }

            var route = localConfigs.FirstOrDefault(i => i.Default)?.Route;
            if (route == null)
            {
                Logger.Error($"No configuration found!");
                throw new NoConfigurationMatchException();
            }
            return route;
        }

        private string GetCacheKey(HttpRequestMessage request)
        {
            return $"{request.Method.Method}:{request.RequestUri.AbsolutePath}:{request.RequestUri.Host}:{request.Headers.Host}:{request.RequestUri.Scheme}";
        }
        public async Task<RequestRouterConfiguration> GetServiceAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var serviceKey = TryParse(request.Headers.Where(i => i.Key == "X-Service").Select(i => i.Value.FirstOrDefault()).FirstOrDefault());
            if (serviceKey != null) return serviceKey;
            var cacheKey = GetCacheKey(request);
            RequestRouterConfiguration result = null;
            _weakCache.TryGet(cacheKey, out result);
            if (result != null) return result;
            var fragments = request.RequestUri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string host = request.RequestUri.Host;
            string method = request.Method.Method;
            try
            {
                while (fragments.Count > 0)
                {
                    var path = string.Join("/", fragments);
                    var settingsUrl = GetConfigUrl(host, method, path);

                    result = await GetRequestSettingFromPath(settingsUrl, cancellationToken);
                    if (result != null) return result;
                    settingsUrl = GetConfigUrl(method: method, path: path);
                    result = await GetRequestSettingFromPath(settingsUrl, cancellationToken);
                    if (result != null) return result;
                    settingsUrl = GetConfigUrl(path: path);
                    result = await GetRequestSettingFromPath(settingsUrl, cancellationToken);
                    if (result != null) return result;
                    fragments.RemoveAt(fragments.Count - 1);
                }
                result = await GetRequestSettingFromPath(GetConfigUrl(method: method, host: host), cancellationToken);
                if (result != null) return result;
                result = await GetRequestSettingFromPath(GetConfigUrl(host: host), cancellationToken);
                if (result != null) return result;
                result = await GetRequestSettingFromPath(GetConfigUrl(), cancellationToken);
                return result;
            }
            finally
            {
                if (result != null)
                {
                    _weakCache.Set(cacheKey, result);
                }
            }
        }
    }
}