using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus.CommandSystem
{
    public class ToggleGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "toggle";
        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            string outputName = PrintObjectFullname(go);
            go.SetActive(!go.activeSelf);
            return new[] { $"Set {outputName} to {(go.activeSelf ? "active" : "inactive")}" };
        }

        
        
        
    }
}