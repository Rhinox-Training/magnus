namespace Rhinox.Magnus.CommandSystem.Player
{
    [CommandInfo("Kills the loaded player", "Player")]
    public class KillPlayerCommand : IConsoleCommand
    {
        public string CommandName => "kill-player";
        public string Syntax => "kill-player";

        public string[] Execute(string[] args)
        {
            var playerManager = PlayerManager.Instance;
            if(playerManager == null)
                return new[] { "No Player Manager found." };
            
            playerManager.KillLocalPlayer();
            return new[] { "Player killed." };
        }
    }
}