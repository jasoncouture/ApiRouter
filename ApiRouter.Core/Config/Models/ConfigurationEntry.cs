using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using ApiRouter.Core.Interfaces;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Installer;
using Newtonsoft.Json;
using NamedAttribute = ApiRouter.Core.Config.Attributes.NamedAttribute;

namespace ApiRouter.Core.Config.Models
{
    public abstract class ConfigurationEntry
    {
        [JsonProperty("type")]
        public string Type
        {
            get { return GetName(GetType()); }
            set { }
        }

        private static string GetName(Type type)
        {
            return type.GetCustomAttribute<NamedAttribute>()?.Name ?? type.Name;
        }

        public abstract Task<bool> IsMatch(HttpRequestMessage request);
        private static Dictionary<string, Type> _keyTypeMaps = null;

        public static Dictionary<string, Type> KeyTypeMaps => _keyTypeMaps ?? (_keyTypeMaps = GetKeyTypeMapsFromAssemblies(Assembly.GetExecutingAssembly()));

        public static Dictionary<string, Type> GetKeyTypeMapsFromAssemblies(params Assembly[] assemblies)
        {
            var dictionary = assemblies.SelectMany(i => i.ExportedTypes)
                .Where(i => !i.IsAbstract)
                .Where(x => typeof(ConfigurationEntry).IsAssignableFrom(x))
                .Select(type => new
                {
                    Name = GetName(type),
                    Type = type
                }).ToDictionary(i => i.Name, i => i.Type, StringComparer.InvariantCultureIgnoreCase);
            return dictionary;
        }
    }
}
