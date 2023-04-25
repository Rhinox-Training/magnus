using System;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Sets the vsync count", "Application Settings")]
    public class VSyncCount: IConsoleCommand
    {
        public string CommandName => "vsync-count";
        public string[] Execute(string[] args)
        {
            if (args.IsNullOrEmpty())
                return new[] { $"The current vsync count is: {QualitySettings.vSyncCount}" };
            
            if (!int.TryParse(args.First(), out int count))
                return new[] { $"Was unable to parse {args.First()} to an integer" };
            
            QualitySettings.vSyncCount = count;
            return new[] { $"The vsync count is now: {QualitySettings.vSyncCount}" };
        }
    }
}