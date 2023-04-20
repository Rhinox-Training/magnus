using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus.CommandSystem;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class AddComponentsToGameObjectCommand : BaseGameObjectConsoleCommand
    {
        public override string CommandName => "add-components";

        protected override string[] ExecuteFor(GameObject go, string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                return new string[]
                {
                    "Command missing arguments. Command format is: add-component <gameObject name> <component1 name> <component2 name> ..."
                };
            }

            // Create the return string array
            var returnVal = new List<string>(7) { "Created '{go.name}' with components of type: " };
            string components = string.Empty;
            // Loop over the remaining arguments
            for (int i = 0; i < args.Length; i++)
            {
                // Get the component type
                var objectType = args[i];
                Type t = ReflectionUtility.FindTypeExtensively(ref objectType);
                // If the type is not found, add an error message
                if (t == null || !typeof(Component).IsAssignableFrom(t))
                {
                    returnVal.Add($"Component type '{objectType.Take(50)}' not found. (Type: '{t?.Name}')");
                    continue;
                }

                // Add the component to the created game object
                go.AddComponent(t);
                components = string.Concat(components, t.Name + " ");
            }

            // Return the logged strings
            returnVal[0] += components;

            return returnVal.ToArray();
        }
    }
}