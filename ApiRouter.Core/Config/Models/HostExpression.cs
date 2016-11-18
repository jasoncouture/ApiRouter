﻿using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiRouter.Core.Config.Attributes;
using Newtonsoft.Json;

namespace ApiRouter.Core.Config.Models
{
    [DirectiveName("hostRegex")]
    public class HostExpression : ConfigurationEntry
    {
        [JsonProperty("hostRegex")]
        public Regex Regex { get; set; }
        public override Task<bool> IsMatch(HttpRequestMessage request)
        {
            return Task.FromResult(!string.IsNullOrEmpty(request.Headers.Host) && Regex.IsMatch(request.Headers.Host));
        }
    }
}