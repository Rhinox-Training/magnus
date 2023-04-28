using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Magnus.CommandSystem;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Switch to GameMode")]
    public class SwitchToModeConsoleCommand : IConsoleCommand
    {
        public string CommandName => "switch-mode";
        public string Syntax => "switch-mode <mode name>";

        public string[] Execute(string[] args)
        {
            if (GameModeManager.Instance == null)
                return new[] { "GameModeManager not found." };
            
            if (args.IsNullOrEmpty())
                return new[] { "Missing argument: <mode name>" };
            string modeName = args.First();
            bool result = GameModeManager.Instance.SwitchTo(modeName);
            if (result)
                return new[] { $"Switching to GameMode {modeName}" };
            return new[] { $"No GameMode with name {modeName} found, aborting..." };
        }
    }
}