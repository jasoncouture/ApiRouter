using ApiRouter.Core.Config.Attributes;

namespace ApiRouter.Core
{
    [Named("host")]
    public class HostRouterConfiguration : RequestRouterConfigurationBase
    {
        public string Host { get; set; }
    }
}