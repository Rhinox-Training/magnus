using System;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    [CommandInfo("Add components to GameObject","GameObject")]
    public class AddComponentsToGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "add-components";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                return new string[]
                {
                    "Command format is: add-components <gameObject name> <component1 name> <component2 name> ..."
                };
            }

            // Create the return string array
            string successString = $"Created '{go.name}' with these components:";
            string errorString = "These components were not found: ";
            string components = string.Empty;
            bool logMissingComponents = false;

            // Loop over the remaining arguments
            for (int i = 0; i < args.Length; i++)
            {
                // Get the component type
                var objectType = args[i];
                Type t = ReflectionUtility.FindTypeExtensively(ref objectType);
                // If the type is not found, add an error message
                if (t == null || !typeof(Component).IsAssignableFrom(t))
                {
                    errorString += " " + (objectType.Length > 50 ? objectType.Substring(0, 50) : objectType);
                    logMissingComponents = true;
                    continue;
                }

                // Add the component to the created game object
                go.AddComponent(t);
                components = string.Concat(components, t.Name + " ");
            }

            // Return the logged strings
            successString += components;

            // Only return the second string if some components weren't found
            return logMissingComponents ? new[] { successString, errorString } : new[] { successString };
        }
    }
}