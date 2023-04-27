namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Gets the path of the current level", "Level")]
    public class GetActiveLevelCommand : IConsoleCommand
    {
        public string CommandName => "get-active-level";
        public string Syntax => "get-active-level";

        public string[] Execute(string[] args)
        {
            return new[] { LevelLoader.GetActiveScene() };
        }
    }
}