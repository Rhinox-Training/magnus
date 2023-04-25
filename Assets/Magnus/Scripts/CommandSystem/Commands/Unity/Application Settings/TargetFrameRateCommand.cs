using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Sets the target frame rate", "Application Settings")]
    public class TargetFrameRateCommand:IConsoleCommand
    {
        public string CommandName => "target-frame-rate";
        public string[] Execute(string[] args)
        {
            if(args.IsNullOrEmpty())
                return new []{$"The current target framerate is: {Application.targetFrameRate}"};
            
            if(!int.TryParse(args.First(),out int frameRate))
                return new[] { $"Was unable to parse {args.First()} to an integer" };

            Application.targetFrameRate = frameRate;
            return new[] { $"The target frame rate is now: {Application.targetFrameRate}" };
        }
    }
}