using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiRouter.Core.Config.Attributes;
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
}