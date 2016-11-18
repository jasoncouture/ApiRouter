using System.Web.Http;
using ApiRouter.Core.Config.JsonConverters;
using ApiRouter.Core.Interfaces;
using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Newtonsoft.Json;

namespace ApiRouter.Core.Installers
{
    public class ComponentsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<JsonConverter>().ImplementedBy<ConfigurationDirectiveConverter>().LifestyleSingleton())
                .Register(Component.For<ProxyHandler>().ImplementedBy<ProxyHandler>().LifestyleSingleton())
                .Register(Component.For<IServiceParser>().ImplementedBy<ServiceParser>().LifestyleSingleton())
                .Register(Component.For<HttpConfiguration>().ImplementedBy<HttpConfiguration>())
                .Register(Component.For<WebApiConfigurationStartupModule>().ImplementedBy<WebApiConfigurationStartupModule>().StartUsingMethod(t => t.Start))
                .Register(Component.For<ServiceHeartbeatModule>().ImplementedBy<ServiceHeartbeatModule>().StartUsingMethod(t => t.Start).StopUsingMethod(t => t.Stop))
                .Register(Component.For<JsonSerializerSettings>().ImplementedBy<JsonSerializerSettings>().LifestyleSingleton())
                .Register(Component.For<JsonSerializer>().UsingFactoryMethod(kernel => JsonSerializer.Create(kernel.Resolve<JsonSerializerSettings>())).LifestyleTransient())
                .Register(Component.For<IWeakCache>().ImplementedBy<WeakCache>().LifestyleSingleton())
                .Register(Component.For<IConfiguration>().ImplementedBy<Configuration>().LifestyleSingleton());
        }
    }
}