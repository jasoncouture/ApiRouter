using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiRouter.Core;
using ApiRouter.Core.Config.Models;
using ApiRouter.Test.Properties;
using Castle.Windsor;
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
            using (var memoryStream = new MemoryStream(Resources.RequestDecodeTestPass))
            using (var streamReader = new StreamReader(memoryStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = _container.Resolve<JsonSerializer>();
                var config = serializer.Deserialize<RouterConfiguration>(jsonReader);
                Assert.IsNotNull(config?.Config);
                Assert.AreEqual(3, config.Config.Count);
                Assert.IsNotNull(config.Config[0]?.Rule);
                Assert.IsInstanceOf<HostConfiguration>(config.Config[0].Rule);
                Assert.IsNotNull(config.Config[1]);
                Assert.IsNull(config.Config[1].Rule);
                Assert.IsNotNull(config.Config[2].Rule);
                Assert.IsInstanceOf<AnyConfigurationEntry>(config.Config[2].Rule);   
            }
        }
    }
}
