using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus.CommandSystem
{
    public abstract class BaseGameObjectConsoleCommand : IConsoleCommand
    {
        public abstract string CommandName { get; }
        
        public virtual string[] Execute(string[] args)
        {
            if (args.IsNullOrEmpty())
                return new[] { "Missing argument <gameObject name>" };

            var objectName = args.First();
            GameObject go = FindGameObject(objectName);
            if (go == null)
                return new [] { $"GameObject with name '{objectName}' not found." };
            return ExecuteFor(go);
        }

        protected abstract string[] ExecuteFor(GameObject go);

        protected virtual GameObject FindGameObject(string objectName)
        {
            var obj = GameObject.Find(objectName);
            if (obj == null)
                obj = Find(objectName);
            return obj;
        }

        protected string PrintObjectFullname(GameObject go)
        {
            return $"{go.GetFullName()} (Scene: {go.scene.name})";
        }
        
        private static GameObject Find(string search)
        {
            var scene = SceneManager.GetActiveScene();
            var sceneRoots = scene.GetRootGameObjects();

            GameObject result = null;
            foreach(var root in sceneRoots)
            {
                if(root.name.Equals(search)) return root;

                result = FindRecursive(root, search);

                if(result) break;
            }

            return result;
        }

        private static GameObject FindRecursive(GameObject obj, string search)
        {
            GameObject result = null;
            foreach(Transform child in obj.transform)
            {
                if(child.name.Equals(search)) return child.gameObject;

                result = FindRecursive (child.gameObject, search);

                if(result) break;
            }

            return result;
        }
    }
}