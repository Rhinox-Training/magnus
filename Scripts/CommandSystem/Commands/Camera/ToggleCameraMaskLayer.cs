using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Toggles a layer in the culling mask of the main camera", "Camera")]
    public class ToggleCameraMaskLayer : IConsoleCommand
    {
        public string CommandName => "toggle-camera-mask-layer";
        public string Syntax => "toggle-camera-mask-layer <layer>";

        public string[] Execute(string[] args)
        {
            if (Camera.main == null)
                return new[] { "No main camera found" };

            var mainCamera = Camera.main;

            if (!UnityTypeParser.TryParseLayer(args[0], out var layerIdx))
                return new[] { $"Unable to parse layer from {args[0]}" };

            int cullingMask = mainCamera.cullingMask;
            cullingMask ^= (1 << layerIdx);
            mainCamera.cullingMask = cullingMask;

            string layerName = LayerMask.LayerToName(layerIdx);
            string returnString = $"Layer {layerIdx}: {layerName} is";

            return (cullingMask & (1 << layerIdx)) != 0
                ? new[] { returnString + " now rendered." }
                : new[] { returnString + " not rendered anymore." };
        }
    }
}