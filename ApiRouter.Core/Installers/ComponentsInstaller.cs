using System.Web.Http;
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
            container.Register(Component.For<ProxyHandler>().ImplementedBy<ProxyHandler>().LifestyleSingleton());
            container.Register(Component.For<IServiceParser>().ImplementedBy<ServiceParser>().LifestyleSingleton());
            container.Register(Component.For<HttpConfiguration>().ImplementedBy<HttpConfiguration>());
            container.Register(Component.For<WebApiConfigurationStartupModule>().ImplementedBy<WebApiConfigurationStartupModule>().StartUsingMethod(t => t.Start));
            container.Register(Component.For<ServiceHeartbeatModule>().ImplementedBy<ServiceHeartbeatModule>().StartUsingMethod(t => t.Start).StopUsingMethod(t => t.Stop));
            container.Register(Component.For<JsonSerializerSettings>().ImplementedBy<JsonSerializerSettings>().LifestyleSingleton());
            container.Register(Component.For<JsonSerializer>().UsingFactoryMethod(kernel => JsonSerializer.Create(kernel.Resolve<JsonSerializerSettings>())).LifestyleTransient());
            container.Register(Component.For<IWeakCache>().ImplementedBy<WeakCache>().LifestyleSingleton());
            container.Register(Component.For<IConfiguration>().ImplementedBy<Configuration>().LifestyleSingleton());
        }
    }
}