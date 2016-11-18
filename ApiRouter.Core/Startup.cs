using Castle.Facilities.Startable;
using Castle.Windsor.Installer;
using Container4AspNet.WebApi;
using Container4AspNet.Windsor;
using Owin;

namespace ApiRouter.Core
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseWindsor(Router.LazyContainer.Value, c => { });
            appBuilder.UseWebApi();
        }
    }
}