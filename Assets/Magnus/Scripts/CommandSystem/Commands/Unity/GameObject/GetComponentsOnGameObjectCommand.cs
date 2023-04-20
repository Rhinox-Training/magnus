using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Get all components on a GameObject and (if desired) its children", "GameObject")]
    public class GetComponentsOnGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "get-components";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            var components = go.GetComponents<Component>();
            string resultString = $"The following components were found on {go.name}: ";
            foreach (Component component in components)
            {
                resultString += component.GetType().Name + " ";
            }
            return new[] { resultString };
        }
    }
}