using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ApiRouter.Core.Interfaces;
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
        public static IDisposable Start()
        {
            DateTimeOffset start = DateTimeOffset.Now;
            var logger = LazyContainer.Value.Resolve<ILogger>();
            var ret = new AggregateDisposable();
            try
            {
                var configurationProvider = LazyContainer.Value.Resolve<INodeConfigurationProvider>();
                var configurations = configurationProvider.GetConfigurationAsync().GetAwaiter().GetResult().ToList();
                if(configurations.Count == 0) throw new InvalidOperationException("No listeners are configured!");
                foreach (var configuration in configurations)
                {
                    if (configuration.Urls.Count == 0)
                    {
                        logger.Info($"Starting listener {configuration.ServerFactory ?? "default"} on port: {configuration.Port}");
                    }
                    else
                    {
                        logger.Info($"Starting listener {configuration.ServerFactory ?? "default"} with URLs: {string.Join(", ", configuration.Urls)}");
                    }
                    ret.Add(WebApp.Start<Startup>(configuration));
                }
            }
            catch(Exception ex)
            {
                var message = $"Startup failed: {ex.Message}";
                logger.Fatal(message, ex);
                throw new Exception("Startup Failed", ex);
            }
            logger.Info($"Startup took: {DateTimeOffset.Now - start}");
            return ret;
        }

        private class AggregateDisposable : IDisposable
        {
            private readonly List<IDisposable> _disposables = new List<IDisposable>();
            public void Add(IDisposable next)
            {
                _disposables.Add(next);
            }

            public void Dispose()
            {
                List<Exception> exceptions = new List<Exception>();
                foreach (var item in _disposables)
                {
                    try
                    {
                        item.Dispose();
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
            }
        }
    }
}
