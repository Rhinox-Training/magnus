using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class FindGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "find";
        
        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            return new [] { $"{PrintObjectFullname(go)}",
                $"   position: {go.transform.position.Print()}" ,
                $"   rotation: {go.transform.rotation.Print()}" ,
                $"   scale:    {go.transform.lossyScale.Print()}" };
        }
    }
}