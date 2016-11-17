using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Consul;

namespace ApiRouter.Core
{
    public class ConsulInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IServiceResolver>().ImplementedBy<ConsulServiceResolver>().LifestyleTransient());
            container.Register(Component.For<IConsulClient>().ImplementedBy<ConsulClient>().LifestyleTransient());
        }
    }
}