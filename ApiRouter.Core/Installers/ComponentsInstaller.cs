using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ApiRouter.Core.Config.JsonConverters;
using ApiRouter.Core.Handlers;
using ApiRouter.Core.Interfaces;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiRouter.Core.Installers
{
    public class ComponentsInstaller : IWindsorInstaller
    {
        private static JsonSerializerSettings CreateSerializerSettings(IKernel kernel)
        {
            var config = new JsonSerializerSettings()
            {
                Converters =
                {
                    new BinaryConverter(),
                    new StringEnumConverter(),
                    new KeyValuePairConverter(),
                    new RegexConverter(),
                    new VersionConverter()
                }
            };
            foreach (var converter in kernel.ResolveAll<JsonConverter>())
            {
                config.Converters.Add(converter);
            }

            return config;
        }
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<ProxyHandler>().ImplementedBy<ProxyHandler>().LifestyleSingleton())
                .Register(Component.For<HttpConfiguration>().UsingFactoryMethod(() => new HttpConfiguration()).LifestyleSingleton())
                .Register(Component.For<WebApiConfigurationStartupModule>().ImplementedBy<WebApiConfigurationStartupModule>().StartUsingMethod(t => t.Start))
                .Register(Component.For<ServiceHeartbeatModule>().ImplementedBy<ServiceHeartbeatModule>().StartUsingMethod(t => t.Start).StopUsingMethod(t => t.Stop))
                .Register(Component.For<JsonSerializerSettings>().ImplementedBy<JsonSerializerSettings>().UsingFactoryMethod(CreateSerializerSettings).LifestyleSingleton())
                .Register(Component.For<JsonConverter>().ImplementedBy<ConfigurationDirectiveConverter>().LifestyleSingleton())
                .Register(Component.For<JsonConverter>().ImplementedBy<ReuqestRouterConfigurationConverter>().LifestyleSingleton())
                .Register(Component.For<ConfigurationStartupModule>().ImplementedBy<ConfigurationStartupModule>().StartUsingMethod(i => i.Start))
                .Register(Component.For<JsonSerializer>().UsingFactoryMethod(kernel => JsonSerializer.Create(kernel.Resolve<JsonSerializerSettings>())).LifestyleTransient())
                .Register(Component.For<IRequestHandler>().ImplementedBy<RequestHandler>().LifestyleSingleton().Named("RequestHandler"))
                .Register(Component.For<IConfigurationReader>().ImplementedBy<ConsulConfigurationReader>().LifestyleSingleton())
                .Register(Classes.FromThisAssembly().BasedOn<IRequestHandlerModule>().WithServiceAllInterfaces().LifestyleSingleton());

            container.Register(
                Component.For<HttpClientHandler>()
                    .UsingFactoryMethod(k => new HttpClientHandler
                    {
                        AllowAutoRedirect = false,
                        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                    }).LifestyleTransient());

            container.Register(
                Component.For<HttpClient>()
                    .UsingFactoryMethod(i => new HttpClient(i.Resolve<HttpClientHandler>(), true)
                    {
                        Timeout = TimeSpan.FromHours(2)
                    }).LifestyleTransient().Named("HttpClient"));

            container
                .Register(Component.For<IHttpClientFactory>().AsFactory())
                .Register(Component.For<IRequestHandlerFactory>().AsFactory());

        }
    }
}