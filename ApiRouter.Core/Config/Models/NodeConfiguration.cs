using System.Collections.Generic;
using Microsoft.Owin.Hosting;

namespace ApiRouter.Core.Config.Models
{
    public class NodeConfiguration
    {
        public string ServerFactory { get; set; }
        public int? Port { get; set; }
        public List<string> Urls { get; set; } = new List<string>();
        public Dictionary<string, string> OwinConfiguration { get; set; } = new Dictionary<string, string>();

        public StartOptions ToStartOptions()
        {
            var ret = new StartOptions
            {
                Port = Port,
                ServerFactory = string.IsNullOrWhiteSpace(ServerFactory) ? null : ServerFactory
            };

            foreach (var kvp in OwinConfiguration ?? new Dictionary<string, string>())
            {
                ret.Settings[kvp.Key] = kvp.Value;
            }
            foreach (var url in Urls)
            {
                ret.Urls.Add(url);
            }

            return ret;
        }

        public static implicit operator StartOptions(NodeConfiguration nodeConfiguration)
        {
            return nodeConfiguration?.ToStartOptions();
        }

        public static NodeConfiguration Default = new NodeConfiguration
        {
            ServerFactory = "Nowin",
            Port = 8080
        };
    }
}