using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class ScaleGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "scale";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if (args.IsNullOrEmpty() || (args.Length != 1 && args.Length != 3))
            {
                return new[]
                {
                    "Command signatures are: ",
                    "scale <GameObject name> <X scale> <Y scale> <Z scale>",
                    "scale <GameObject name> <uniform scale>"
                };
            }
            Vector3 scale = go.transform.localScale;
            
            if (args.Length == 1)
            {
                if (!float.TryParse(args[0], out var val))
                {
                    return new[]
                        { "Invalid uniform scale value" };
                }
                scale += new Vector3(val, val, val);
            }
            else if (args.Length >= 3)
            {
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

                scale += new Vector3(x, y, z);
            }

            go.transform.localScale = scale;
            return new[] { $"Scaled {go.name} to ({scale.x}, {scale.y}, {scale.z})" };
        }
    }
}