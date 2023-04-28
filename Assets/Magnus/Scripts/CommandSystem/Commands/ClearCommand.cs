using System;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Clears the console", "Console")]
    public class ClearCommand : IConsoleCommand
    {
        public string CommandName => "clear";
        public string Syntax => CommandName;

        public string[] Execute(string[] args)
        {
            ConsoleCommandManager.Instance.ClearConsole();
            return Array.Empty<string>();
        }
    }
}