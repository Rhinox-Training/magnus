using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Sets the euler rotation of a GameObject", "Transform")]
    public class SetRotationOfGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "set-rotation";
        public override string Syntax=>"set-rotation <GameObject name> <X angle> <Y angle> <Z angle>";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if (args.IsNullOrEmpty() || args.Length < 3)
            {
                return new[]
                {
                    $"Command signature is: {Syntax}"
                };
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

            var eulerAngles = new Vector3(x, y, z);
            go.transform.eulerAngles = eulerAngles;
            return new[] { $"Rotated {go.name} to ({eulerAngles.x}, {eulerAngles.y}, {eulerAngles.z})" };
        }
    }
}