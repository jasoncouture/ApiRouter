using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiRouter.Core.Config.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DirectiveNameAttribute : Attribute
    {
        public DirectiveNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
