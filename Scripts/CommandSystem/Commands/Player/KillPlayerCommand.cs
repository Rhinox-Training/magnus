namespace Rhinox.Magnus.CommandSystem.Player
{
    [CommandInfo("Kills the loaded player", "Player")]
    public class KillPlayerCommand : IConsoleCommand
    {
        public string CommandName => "kill-player";
        public string Syntax => "kill-player";

        public string[] Execute(string[] args)
        {
            PlayerManager.Instance.KillPlayer();
            return new[] { "Player killed." };
        }
    }
}