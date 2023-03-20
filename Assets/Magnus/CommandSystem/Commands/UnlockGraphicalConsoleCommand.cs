using System.Linq;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.CommandSystem
{
    [HiddenCommand]
    public class UnlockGraphicalConsoleCommand : IConsoleCommand
    {
        public string CommandName => "enable-dev";
        
        public string[] Execute(string[] args)
        {
            if (args.IsNullOrEmpty())
                return new[] { "Missing argument: <developer token>" };

            string devToken = args.First();
            if (CheckWithSecret(devToken))
            {
                ConsoleCommandManager.Instance.EnableGUIAccess();
                return new[] { "Developer access enabled!" };
            }

            return new[] { "Developer token is invalid!" };
        }

        private bool CheckWithSecret(string devToken)
        {
            var secret = MagnusConfig.Instance.CommandSystemSecret;
            if (string.IsNullOrWhiteSpace(secret)) // Always allow if no secret was configured
                return true;

            if (string.IsNullOrWhiteSpace(devToken))
                return false;
            
            secret = secret.Trim();
            return secret.Equals(devToken.Trim());
        }
    }
}