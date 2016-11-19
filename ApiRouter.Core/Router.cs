using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Castle.Core;
using Castle.Core.Logging;
using Castle.Facilities.Logging;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;


namespace ApiRouter.Core
{
    public static class Router
    {
        internal static Lazy<IWindsorContainer> LazyContainer = new Lazy<IWindsorContainer>(() =>
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
            container.AddFacility<TypedFactoryFacility>();
            container.AddFacility<LoggingFacility>(f => f.UseNLog());
            container.AddFacility<StartableFacility>(f => f.DeferredStart());
            container.Install(FromAssembly.This());
            return container;
        });
        public static IDisposable Start(int? port = null)
        {
            var logger = LazyContainer.Value.Resolve<ILogger>();
            logger.Info($"ApiRouter 1.0 starting up on port #{port}");
            var options = new StartOptions()
            {
                ServerFactory = "Nowin",
                Port = port
            };
            try
            {
                return WebApp.Start<Startup>(options);
            }
            catch(Exception ex)
            {
                var message = $"Startup failed: {ex.Message}";
                logger.Fatal(message, ex);
                throw new Exception("Startup Failed", ex);
            }
        }
    }
}
