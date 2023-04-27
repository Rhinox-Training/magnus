using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Returns all rendered layers on the main camera", "Camera")]
    public class GetCullingMaskCommand : IConsoleCommand
    {
        public string CommandName => "get-rendered-layers";
        public string Syntax => "get-rendered-layers";

        public string[] Execute(string[] args)
        {
            if (CameraInfo.Instance == null)
                return new[] { "Camera Info is not loaded." };

            LayerMask layerMask = CameraInfo.Instance.Main.cullingMask;

            string layers = "";

            // Iterate through all possible layers (0 to 31)
            for (int i = 0; i < 32; i++)
            {
                // Shift the layer mask by the current index
                int shiftedLayer = 1 << i;

                // Check if the current layer is rendered by the camera
                if ((layerMask & shiftedLayer) == shiftedLayer)
                {
                    // Get the name of the layer and add it to the layers string
                    string layerName = LayerMask.LayerToName(i);
                    if (layerName.IsNullOrEmpty()) continue;

                    layers += string.IsNullOrEmpty(layers) ? layerName : ", " + layerName;
                }
            }

            return new[] { layers };
        }
    }
}