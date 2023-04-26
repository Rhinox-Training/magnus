using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Lists all named layers", "Layers")]
    public class ListLayersCommand:IConsoleCommand
    {
        public string CommandName => "list-layers";
        public string Syntax => CommandName;
        public string[] Execute(string[] args)
        {
            string layerNames = "";
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if(!layerName.IsNullOrEmpty())
                    layerNames += LayerMask.LayerToName(i) + " ";
            }
            
            return new[] { layerNames };
        }
    }
}