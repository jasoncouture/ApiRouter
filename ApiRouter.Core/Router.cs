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
        public static IDisposable Start(string url)
        {
            return WebApp.Start<Startup>(url);
        }
    }
}
