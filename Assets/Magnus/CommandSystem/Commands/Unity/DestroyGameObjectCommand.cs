using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class DestroyGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "destroy";
        
        protected override string[] ExecuteFor(GameObject go)
        {
            string outputName = PrintObjectFullname(go);
            Utility.Destroy(go);
            return new[] { $"Destroyed {outputName}" };
        }
    }
}