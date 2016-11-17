using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiRouter.Core;

namespace ApiRouter.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Router.Start("http://+:8080/"))
            {
                System.Console.ReadLine();
            }
        }
    }
}
