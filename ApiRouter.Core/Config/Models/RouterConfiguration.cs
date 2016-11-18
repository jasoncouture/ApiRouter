using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    public sealed class RouterConfiguration
    {
        [JsonProperty("routingConfig")]
        public List<ConfigContainer> Config { get; } = new List<ConfigContainer>();


    }
}