using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using ApiRouter.Core.Config.Attributes;
using ApiRouter.Core.Config.Models;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;

namespace ApiRouter.Core
{
    public abstract class RequestRouterConfigurationBase
    {
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public int? Port { get; set; }
        public string Scheme { get; set; }
        [JsonProperty("type")]
        public string Type
        {
            get { return GetName(GetType()); }
            set { }
        }

        private static string GetName(Type type)
        {
            return NamedAttribute.GetNameFromType(type) ?? type.Name;
        }
        private static Dictionary<string, Type> _keyTypeMaps = null;

        public static Dictionary<string, Type> KeyTypeMaps => _keyTypeMaps ?? (_keyTypeMaps = GetKeyTypeMapsFromAssemblies(Assembly.GetExecutingAssembly()));

        public static Dictionary<string, Type> GetKeyTypeMapsFromAssemblies(params Assembly[] assemblies)
        {
            var dictionary = assemblies.SelectMany(i => i.ExportedTypes)
                .Where(i => !i.IsAbstract)
                .Where(x => typeof(RequestRouterConfigurationBase).IsAssignableFrom(x))
                .Select(type => new
                {
                    Name = GetName(type),
                    Type = type
                }).ToDictionary(i => i.Name, i => i.Type, StringComparer.InvariantCultureIgnoreCase);
            return dictionary;
        }

    }
    [Named("statusCode")]
    public class ResponseCodeRouterConfiguration : RequestRouterConfigurationBase
    {
        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; }
    }

    [Named("host")]
    public class HostRouterConfiguration : RequestRouterConfigurationBase
    {
        public string Host { get; set; }
    }

    [Named("consulService")]
    public class ConsulServiceRouterConfiguration : RequestRouterConfigurationBase
    {
        private string _tag;
        [JsonProperty("service")]
        public string Service { get; set; }
        [JsonProperty("tag")]
        public string Tag
        {
            get { return _tag; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                _tag = value;
            }
        }
    }

    [Named("__default")]
    public class RequestRouterConfiguration : RequestRouterConfigurationBase
    {
        private string _tag;
        [JsonProperty("service")]
        public string Service { get; set; }
        [JsonProperty("tag")]
        public string Tag
        {
            get { return _tag; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                _tag = value;
            }
        }
        [JsonProperty("hostHeader")]
        public string HostHeader { get; set; }
        [JsonProperty("host")]
        public string Host { get; set; }
    }
}