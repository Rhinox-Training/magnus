using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Rename a GameObject", "GameObject")]
    public class RenameGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "rename";
        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if(args.IsNullOrEmpty())
                return new[] { "Command syntax is: rename <GameObject name> <new name>" };
            string oldName = go.name;
            string newName = args.First();
            go.name = newName;
            return new[] { $"Renamed {oldName} to {newName}" };
        }
    }
}