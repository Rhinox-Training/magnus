using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Translate a GameObject", "Transform")]
    public class TranslateGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "translate";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if (args.IsNullOrEmpty() || args.Length < 3)
            {
                return new[]
                    { "Command signature is: translate <GameObject name> <Translate X> <Translate Y> <Translate Z>" };
            }
            
            if(!float.TryParse(args[0], out var x))
            {
                return new[]
                    { "Invalid X value" };
            }
            
            if(!float.TryParse(args[1], out var y))
            {
                return new[]
                    { "Invalid Y value" };
            }
            
            if(!float.TryParse(args[2], out var z))
            {
                return new[]
                    { "Invalid Z value" };
            }
            
            go.transform.Translate(x, y, z);

            var position = go.transform.position;
            return new[]{$"Translated {go.name} to ({position.x}, {position.y}, {position.z})"};
        }
    }
}