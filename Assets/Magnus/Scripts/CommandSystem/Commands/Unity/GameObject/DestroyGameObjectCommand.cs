using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Destroy a target GameObject", "GameObject")]
    public class DestroyGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "destroy";
        public override string Syntax => "destroy <GameObject>";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            string outputName = PrintObjectFullname(go);
            Utility.Destroy(go);
            return new[] { $"Destroyed {outputName}" };
        }
    }
}