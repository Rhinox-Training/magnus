﻿using Rhinox.Magnus.CommandSystem;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("List the currently registered GameModes")]
    public class GameModeListConsoleCommand : IConsoleCommand
    {
        public string CommandName => "list-modes";
        public string Syntax => "list-modes";

        public string[] Execute(string[] args)
        {
            if(GameModeManager.Instance == null)
                return new[] { "GameModeManager not found." };
            
            return new[] { string.Join(", ", GameModeManager.Instance.GetModeNames()) };
        }
    }
}