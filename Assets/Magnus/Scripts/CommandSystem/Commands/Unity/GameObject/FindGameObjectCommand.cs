using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Find a GameObject by name and get its Transform", "GameObject")]
    public class FindGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "find";
        public override string Syntax => "find <name>";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            return new [] { $"{PrintObjectFullname(go)}",
                $"   position: {go.transform.position.Print()}" ,
                $"   rotation: {go.transform.rotation.Print()}" ,
                $"   scale:    {go.transform.lossyScale.Print()}" };
        }
    }
}