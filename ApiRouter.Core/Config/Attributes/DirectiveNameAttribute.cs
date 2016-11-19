using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiRouter.Core.Config.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NamedAttribute : Attribute
    {
        public NamedAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static string GetNameFromType(Type type)
        {
            return type.GetCustomAttribute<NamedAttribute>()?.Name;
        }
    }
}
