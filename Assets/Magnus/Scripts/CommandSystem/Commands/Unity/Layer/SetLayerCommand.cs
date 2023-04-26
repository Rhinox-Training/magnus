using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Sets the layer of a GameObject using the LayerManager", "Layers")]
    public class SetLayerCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "set-layer";
        public override string Syntax => "set-layer <name> <layer>";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if (args.IsNullOrEmpty())
                return new[] { "Missing argument <layer>" };
            
            // Check if the given string is an integer
            if (int.TryParse(args[0], out int layerIdx))
            {
                // Check if the layerIdx is in the range of the layer mask
                if (layerIdx < 0 || layerIdx >= 31)
                    return new[] { $"LayerIndex '{args[0]}' is not valid. Should be in the range [0;31]." };
            }
            else
                layerIdx = LayerMask.NameToLayer(args[0]);

            if (layerIdx == -1)
                return new[] { $"Layer '{args[0]}' not found." };

            LayerManager.Instance.SetLayer(go, layerIdx);

            return new[] { $"Layer of {go.name} set to {layerIdx}" };
        }
    }
}