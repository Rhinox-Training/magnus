using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Set the screen resolution","Screen")]
    public class SetResolutionCommand : IConsoleCommand
    {
        public string CommandName => "set-resolution";
        public string Syntax => "set-resolution <x> <y>";

        public string[] Execute(string[] args)
        {
            if(args.IsNullOrEmpty())
                return new[] { "The current resolution is: "+ Screen.currentResolution };
            
            if(args.Length <2)
                return new string[] { $"Usage: {CommandName} <x> <y>" };
            
            if(!int.TryParse(args[0], out int x))
                return new[] { $"Unable to parse x from {args[0]}" };
            if(!int.TryParse(args[1], out int y))
                return new[] { $"Unable to parse y from {args[10]}" };
            
            Screen.SetResolution(x,y,Screen.fullScreenMode);
            return new[] { $"The current resolution is: {x}x{y}" };
        }
    }
}