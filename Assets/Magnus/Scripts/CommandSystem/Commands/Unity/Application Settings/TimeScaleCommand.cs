using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Magnus.CommandSystem;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Changes the TimeScale of the game", "Application Settings")]
    public class TimeScaleCommand : IConsoleCommand
    {
        public string CommandName => "time-scale";
        public string Syntax => "time-scale <scale>";

        public string[] Execute(string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                return new[] { $"The current Time Scale is {Time.timeScale}" };
            }

            if (!float.TryParse(args.First(), out float newScale))
                return new[] { $"Was unable to parse \"{args.First()}\" to a float." };

            Time.timeScale = newScale;
            return new[] { $"The new Time Scale is {Time.timeScale}" };
        }
    }
}