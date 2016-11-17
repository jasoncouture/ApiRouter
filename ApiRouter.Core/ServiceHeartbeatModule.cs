using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Consul;

namespace ApiRouter.Core
{
    public class ServiceHeartbeatModule
    {
        private readonly IConsulClient _consulClient;
        private readonly Timer _serivceHeartbeatTimer;
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public ServiceHeartbeatModule(IConsulClient consulClient)
        {
            _consulClient = consulClient;
            _serivceHeartbeatTimer = new Timer(OnTimerTick);
        }

        private void OnTimerTick(object state)
        {
            OnTimerTickAsync(state).GetAwaiter().GetResult();
        }

        private async Task OnTimerTickAsync(object state)
        {
            var result = await _consulClient.Agent.ServiceRegister(new AgentServiceRegistration
            {
                ID = "requestrouter",
                Name = "requestrouter",
                Port = 8080,
                Check = new AgentServiceCheck()
                {
                    TTL = TimeSpan.FromSeconds(10),
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                }
            });
            Logger.Debug($"Consul service register response code: {result.StatusCode} in {result.RequestTime}");
            await _consulClient.Agent.PassTTL("service:requestrouter", $"Process ID: {Process.GetCurrentProcess().Id}");
        }

        public void Start()
        {
            Logger.Info("Service registration startup");
            _serivceHeartbeatTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public void Stop()
        {
            _serivceHeartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Logger.Info("Service registration shutdown");
        }
    }
}