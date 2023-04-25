using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Sets the gravity of the application", "Application")]
    public class GravityCommand:IConsoleCommand
    {
        public string CommandName => "gravity";
        public string Syntax => "gravity <x> <y> <z>";

        public string[] Execute(string[] args)
        {
            if(args.IsNullOrEmpty())
                return new string[] { $"Current gravity is: {Physics.gravity}" };
            
            if(args.Length < 3)
                return new string[] { $"Usage: {CommandName} <x> <y> <z>" };
            
            if(!float.TryParse(args.First(), out float gravityX))
                return new string [] { $"Was unable to parse a float from {args.First()}" };
            if (!float.TryParse(args[1], out float gravityY))
                return new string[] { $"Was unable to parse a float from {args[1]}" };
            if (!float.TryParse(args[2], out float gravityZ))
                return new string[] { $"Was unable to parse a float from {args[1]}" };
            
            Physics.gravity = new Vector3(gravityX,gravityY,gravityZ);
            return new string[] { $"Gravity set to {Physics.gravity}" };
        }
    }
}