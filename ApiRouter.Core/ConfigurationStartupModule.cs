using System;
using System.Threading;
using ApiRouter.Core.Interfaces;
using Castle.Core.Logging;

namespace ApiRouter.Core
{
    public class ConfigurationStartupModule
    {
        private readonly IConfigurationReader _configurationReader;
        public ILogger Logger { get; set; }
        public ConfigurationStartupModule(IConfigurationReader configurationReader)
        {
            _configurationReader = configurationReader;
        }

        public void Start()
        {
            try
            {
                _configurationReader.GetConfiguration(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load configuration: {ex.Message}, Will retry automatically as requests come in", ex);
                Logger.Warn("Server will return BadGateway until successful.", ex);
            }
        }
    }
}