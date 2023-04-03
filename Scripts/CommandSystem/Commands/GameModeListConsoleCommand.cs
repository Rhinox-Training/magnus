using Rhinox.Magnus.CommandSystem;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("List the currently registered GameModes")]
    public class GameModeListConsoleCommand : IConsoleCommand
    {
        public string CommandName => "list-modes";
        
        public string[] Execute(string[] args)
        {
            return new[] { string.Join(", ", GameModeManager.Instance.GetModeNames()) };
        }
    }
}