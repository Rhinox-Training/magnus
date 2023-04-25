using System.Linq;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.CommandSystem
{
    [HiddenCommand]
    public class UnlockGraphicalConsoleCommand : IConsoleCommand
    {
        public string CommandName => "enable-dev";
        public string Syntax => "enable-dev <developer token>";

        public string[] Execute(string[] args)
        {
            if (HasCommandSystemSecretConfigured() && args.IsNullOrEmpty())
                return new[] { "Missing argument: <developer token>" };

            string devToken = args.FirstOrDefault();
            if (CheckWithSecret(devToken))
            {
                ConsoleCommandManager.Instance.EnableGUIAccess();
                return new[] { "Developer access enabled!" };
            }

            return new[] { "Developer token is invalid!" };
        }

        private bool CheckWithSecret(string devToken)
        {
            if (!HasCommandSystemSecretConfigured()) // Always allow if no secret was configured
                return true;

            if (string.IsNullOrWhiteSpace(devToken))
                return false;
            
            var secret = MagnusProjectSettings.Instance.CommandSystemSecret;
            secret = secret.Trim();
            return secret.Equals(devToken.Trim());
        }

        private static bool HasCommandSystemSecretConfigured()
        {
            var secret = MagnusProjectSettings.Instance.CommandSystemSecret;
            return !string.IsNullOrWhiteSpace(secret);
        }
    }
}