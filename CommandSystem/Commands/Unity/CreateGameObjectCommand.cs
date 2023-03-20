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
            if (args.IsNullOrEmpty())
                return new[] { "Missing argument <gameObject name>" };

            var objectType = args.First();
            Type t = ReflectionUtility.FindTypeExtensively(ref objectType, false);
            
            if (t == null || !typeof(Component).IsAssignableFrom(t))
                return new [] { $"Component type '{objectType.Take(50)}' not found. (Type: '{t?.Name}')" };
            
            string objectName = "New GameObject";
            if (args.Length > 1)
                objectName = args[1];
            GameObject go = new GameObject(objectName);
            go.AddComponent(t);

            return new[] { $"Created '{objectName}' with Component of type '{t.FullName}'." };
        }
    }
}