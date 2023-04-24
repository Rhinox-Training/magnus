using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class SetPositionOfGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "set-position";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if (args.IsNullOrEmpty() || args.Length < 3)
            {
                return new[]
                    { "Command signature is: set-position <GameObject name> <Translate X> <Translate Y> <Translate Z>" };
            }

            if (!float.TryParse(args[0], out var x))
            {
                return new[]
                    { "Invalid X value" };
            }

            if (!float.TryParse(args[1], out var y))
            {
                return new[]
                    { "Invalid Y value" };
            }

            if (!float.TryParse(args[2], out var z))
            {
                return new[]
                    { "Invalid Z value" };
            }

            var position = new Vector3(x,y,z);
            go.transform.position = position;
            
            return new[] { $"Set the position of {go.name} to ({position.x}, {position.y}, {position.z})" };
        }
    }
}