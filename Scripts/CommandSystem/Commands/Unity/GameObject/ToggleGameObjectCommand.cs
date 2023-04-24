using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Toggle the active state of a GameObject", "GameObject")]
    public class ToggleGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "toggle";
        public override string Syntax =>"toggle <GameObject name>";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            string outputName = PrintObjectFullname(go);
            go.SetActive(!go.activeSelf);
            return new[] { $"Set {outputName} to {(go.activeSelf ? "active" : "inactive")}" };
        }

        
        
        
    }
}