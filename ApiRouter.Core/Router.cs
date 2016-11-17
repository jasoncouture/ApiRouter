using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Castle.Core;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;

namespace ApiRouter.Core
{
    public static class Router
    {
        public static IDisposable Start(int? port = null)
        {
            var options = new StartOptions()
            {
                ServerFactory = "Nowin",
                Port = port
            };
            return WebApp.Start<Startup>(options);
        }
    }
}
