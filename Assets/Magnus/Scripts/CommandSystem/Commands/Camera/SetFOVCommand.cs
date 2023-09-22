using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Sets the FOV of the main camera", "Camera")]
    public class SetFOVCommand : IConsoleCommand
    {
        public string CommandName => "set-fov";
        public string Syntax => "set-fov <angle (degrees)>>";

        public string[] Execute(string[] args)
        {
            var mainCamera = Camera.main;
            
            if(mainCamera == null)
                return new[] { "No main camera found" };
            
            if (args.IsNullOrEmpty())
            {
                return new[] { $"The current FOV is: {mainCamera.fieldOfView} degrees" };
            }
            
            if(!float.TryParse(args[0], out var fov))
                return new[] { $"Unable to parse float from {args[0]}" };

            mainCamera.fieldOfView = fov;
            return new[] { $"The current FOV is: {mainCamera.fieldOfView} degrees" };
        }
    }
}