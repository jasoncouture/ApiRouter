using Castle.Facilities.Logging;
using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace ApiRouter.Core.Installers
{
    public class ContainerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
            container.AddFacility<LoggingFacility>(f => f.UseNLog());
            container.Register(Component.For<IWindsorContainer>().Instance(container).LifestyleSingleton());
        }
    }
}