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
            if(LayerManager.Instance == null)
                return new[] { "LayerManager not found." };
            
            if (args.IsNullOrEmpty())
                return new[] { "Missing argument <layer>" };
            
            if(!UnityTypeParser.TryParseLayer(args[0], out var layerIdx))
                return new[] { $"Unable to parse layer from {args[0]}" };

            LayerManager.Instance.SetLayer(go, layerIdx);

            return new[] { $"Layer of {go.name} set to {layerIdx}: {LayerMask.LayerToName(layerIdx)}" };
        }
    }
}