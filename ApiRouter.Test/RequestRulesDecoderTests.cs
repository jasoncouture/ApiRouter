using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ApiRouter.Core;
using ApiRouter.Core.Config.Models;
using ApiRouter.Test.Properties;
using Castle.Windsor;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ApiRouter.Test
{
    [TestFixture]
    public class RequestRulesDecoderTests
    {
        private IWindsorContainer _container;
        [OneTimeSetUp]
        public void BootContainer()
        {
            _container = Router.LazyContainer.Value;
        }
        [Test]
        public async Task TestJsonRead()
        {
            var expected = new RouterConfiguration
            {
                Config =
                {
                    new ConfigContainer
                    {
                        Rule = new HostConfiguration
                        {
                            Host = "www.google.com"
                        },
                        Route = new HostRouterConfiguration
                        {
                            Host = "www.google.com",
                            Port = 443,
                            Scheme = "https"
                        },
                        Default = false
                    },
                    new ConfigContainer
                    {
                        Rule = null,
                        Route = new ConsulServiceRouterConfiguration
                        {
                            Service = "consului"
                        },
                        Default = true
                    },
                    new ConfigContainer
                    {
                        Rule = new AnyConfigurationEntry
                        {
                            Children =
                            {
                                new HostsConfiguration
                                {
                                    Hosts = new[] {"*.nightowlautoshop.com", "nightowl.pssproducts.com"}
                                },
                                new PathPrefixConfiguration
                                {
                                    Prefix = "/"
                                }
                            }
                        },
                        Route = new ConsulServiceRouterConfiguration()
                        {
                            Service = "nightowl-dev",
                            Tag = null
                        },
                        Default = false
                    }, new ConfigContainer
                    {
                        Rule = null,
                        Route = new RequestRouterConfiguration
                        {
                            Service = "consului",
                            Tag = null
                        },
                        Default = true
                    },
                    new ConfigContainer
                    {
                        Rule = null,
                        Route = new ResponseCodeRouterConfiguration
                        {
                            StatusCode = HttpStatusCode.NotFound
                        }
                    }
                }
            };
            using (var memoryStream = new MemoryStream(Resources.RequestDecodeTestPass))
            using (var streamReader = new StreamReader(memoryStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = _container.Resolve<JsonSerializer>();
                var config = serializer.Deserialize<RouterConfiguration>(jsonReader);
                config.ShouldBeEquivalentTo(expected);
            }
        }
    }
}
