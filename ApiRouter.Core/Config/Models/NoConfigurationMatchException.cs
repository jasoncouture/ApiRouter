using System;

namespace ApiRouter.Core.Config.Models
{
    public class NoConfigurationMatchException : Exception
    {
        public NoConfigurationMatchException(string message) : base(message) { }
        public NoConfigurationMatchException() : base("No configuration matching the request could be found.") { }
    }
}