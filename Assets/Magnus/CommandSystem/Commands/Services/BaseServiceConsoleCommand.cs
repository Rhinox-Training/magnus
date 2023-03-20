using System.Linq;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.CommandSystem
{
    public abstract class BaseServiceConsoleCommand : IConsoleCommand
    {
        public abstract string CommandName { get; }
        public string[] Execute(string[] args)
        {
            if (args.IsNullOrEmpty())
                return new[] { "Missing argument <service name>" };

            var serviceName = args.First();
            var service = Services.FindService(serviceName);
            
            if (service == null)
                return new [] { $"Service '{serviceName}' not found." };
            return ExecuteFor(service);
        }

        protected abstract string[] ExecuteFor(IService service);
    }
}