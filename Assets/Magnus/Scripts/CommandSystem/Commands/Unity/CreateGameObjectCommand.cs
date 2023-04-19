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
            string[] returnVal = new string[args.Length];
            
            // Loop over the remaining arguments
            for (int i = 1; i < args.Length; i++)
            {
                // Get the component type
                var objectType = args[i];
                Type t = ReflectionUtility.FindTypeExtensively(ref objectType, false);
                // If the type is not found, add an error message
                if (t == null || !typeof(Component).IsAssignableFrom(t))
                {
                    returnVal[i] = $"Component type '{objectType.Take(50)}' not found. (Type: '{t?.Name}')";
                    continue;
                }
                // Add the component to the created game object
                go.AddComponent(t);
            }
            // Return the logged strings
            returnVal[0] = $"Created '{go.name}' with components of type '{string.Join(", ", go.GetComponents<Component>().Select(c => c.GetType().Name))}'.";
            return returnVal;
        }
    }
}