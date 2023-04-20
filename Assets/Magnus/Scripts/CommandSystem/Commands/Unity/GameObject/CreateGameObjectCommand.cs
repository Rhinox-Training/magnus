using System;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

namespace Rhinox.Magnus.CommandSystem
{
    public class CreateGameObjectCommand : IConsoleCommand
    {
        public string CommandName => "create";

        public string[] Execute(string[] args)
        {
            GameObject go = new GameObject();
            // If no arguments are given, return a default empty game object
            if (args.IsNullOrEmpty())
            {
                string objectName = "New GameObject";
                go.name = objectName;
                return new[] { $"Created '{objectName}' with no additional components." };
            }

            go.name = args[0];

            // Create the return string array
            string successString = $"Created '{go.name}' with these components:";
            string errorString = "These components were not found: ";
            string components = string.Empty;
            bool logMissingComponents = false;

            // Loop over the remaining arguments
            for (int i = 1; i < args.Length; i++)
            {
                // Get the component type
                string objectType = args[i];
                Type t = ReflectionUtility.FindTypeExtensively(ref objectType, false);
                // If the type is not found, add an error message
                if (t == null || !typeof(Component).IsAssignableFrom(t))
                {
                    errorString += " " + (objectType.Length > 50 ? objectType.Substring(0, 50) : objectType);
                    logMissingComponents = true;
                    continue;
                }

                // Add the component to the created game object
                go.AddComponent(t);
                components = string.Concat(components, " " + t.Name);
            }

            // Return the logged strings
            successString += components;

            // Only return the second string if some components weren't found
            return logMissingComponents ? new[] { successString, errorString } : new[] { successString };
        }
    }
}