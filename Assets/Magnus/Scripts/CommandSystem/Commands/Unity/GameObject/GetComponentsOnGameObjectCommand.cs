using System;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Get all components on a GameObject and (if desired) its children", "GameObject")]
    public class GetComponentsOnGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "get-components";
        public override string Syntax => "get-components <name> <include-children: true/false>";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            bool includeChildren = false;
            string errorString = "";
            bool isArgIncorrect = false;
            
            if (!args.IsNullOrEmpty())
            {
                if (!bool.TryParse(args[0], out includeChildren))
                {
                    errorString = "Command format is: get-components <GameObject name> <true/false>";
                    isArgIncorrect = true;
                }
            }
            
            // Get all components on the target
            Component[] components = includeChildren ? go.GetComponentsInChildren<Component>() : go.GetComponents<Component>();

            string resultString = $"The following components were found: ";
            foreach (Component component in components)
            {
                resultString += component.GetType().Name + " ";
            }

            return !isArgIncorrect ? new[] { resultString } : new[] { errorString, resultString };
        }
    }
}