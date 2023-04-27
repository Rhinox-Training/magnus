using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Unsets the layer of a GameObject using the LayerManager", "Layers")]
    public class UnsetLayer : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "unset-layer";
        public override string Syntax => "unset-layer <name> <layer>";
        
        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if (LayerManager.Instance == null)
                return new[] { "LayerManager not found." };
            
            if(args.IsNullOrEmpty())
                return new[] { "Missing argument <layer>" };

            if (!int.TryParse(args[0], out int layerIdx))
            {
                layerIdx = LayerMask.NameToLayer(args[0]);
            }

            LayerManager.Instance.UnsetLayer(go, layerIdx);
            
            return new[] { $"Layer of {go.name} unset {layerIdx}" };
        }
    }
}