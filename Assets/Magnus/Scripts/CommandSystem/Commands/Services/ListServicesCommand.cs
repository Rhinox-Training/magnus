using System.Linq;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Lists all available services", "Services")]
    public class ListServicesCommand : IConsoleCommand
    {
        public string CommandName => "list-services";
        public string Syntax => CommandName;

        public string[] Execute(string[] args)
        {
            var services = Services.GetAvailableServices();
            return new[] { string.Join(", ", services.Select(x => x.Name).ToArray()) };
        }
    }
}