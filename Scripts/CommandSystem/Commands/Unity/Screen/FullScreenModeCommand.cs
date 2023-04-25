using System;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Sets the full screen mode", "Screen")]
    public class FullScreenModeCommand: IConsoleCommand
    {
        public string CommandName => "full-screen-mode";
        public string[] Execute(string[] args)
        {
            if(args.IsNullOrEmpty())
                return new [] { "Full screen mode is " + (Screen.fullScreenMode) };
            
            if(!Enum.TryParse(args.First(), out FullScreenMode mode))
                return new [] { $"Unable to parse FullScreenMode from {args.First()}" };
            
            Screen.fullScreenMode = mode;
            return new [] { $"Full screen mode set to {mode}" };
        }
    }
}