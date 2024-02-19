using System;
using System.Diagnostics;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus
{
    public abstract class SceneReferenceResolver : IObjectReferenceResolver
    {
        public SceneReference SceneReference;
        public string Path;

        public string ErrorMessage => $"Object not found: {Path}";
        public string Description => $"Currently bound to: {Path}";

        protected SceneReferenceResolver(Scene scene)
        {
            SceneReference = new SceneReference(scene);
        }

        public abstract Object Resolve();

        protected static string GetPath(Transform transform)
        {
            const string sep = "/";
            
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + sep + path;
            }
            return sep + path; // Start with sep to indicate root level
        }
        
        [ReferenceResolver]
        public static bool TryEncode(UnityEngine.Object o, out IObjectReferenceResolver resolver)
        {
            if (o is GameObject go/* && go.scene.buildIndex >= 0*/)
            {
                resolver = new SceneObjectReferenceResolver(go.scene, go.transform);
                return true;
            }

            if (o is Component comp/* && comp.gameObject.scene.buildIndex >= 0*/)
            {
                resolver = new SceneComponentReferenceResolver(comp.gameObject.scene, comp);
                return true;
            }

            resolver = null;
            return false;
        }

        protected bool Equals(SceneReferenceResolver other)
        {
            if (SceneReference == null)
                return other.SceneReference == null;
                
            return SceneReference.Equals(other.SceneReference) && Path == other.Path;
        }

        public bool Equals(IObjectReferenceResolver obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SceneReferenceResolver) obj);
        }
    }
    
    [Serializable]
    public class SceneObjectReferenceResolver : SceneReferenceResolver
    {
        public SceneObjectReferenceResolver(Scene scene, Transform t) : base(scene)
        {
            Path = GetPath(t);
        }

        public override Object Resolve()
        {
            // Todo actually search in the saved scene
            // var obj = Utility.FindInScene(Path);
            // TODO move FindInScene's loose search to SceneHierarchyTree
            var obj = SceneHierarchyTree.Find(Path, true);
            
            if (obj == null)
                PLog.Warn<MagnusLogger>($"Could not find object '{Path}'");
            return obj;
        }
    }
    
    [Serializable]
    public class SceneComponentReferenceResolver : SceneObjectReferenceResolver
    {
        public string ComponentType;
        
        public SceneComponentReferenceResolver(Scene scene, Component comp) : base(scene, comp.transform)
        {
            ComponentType = comp.GetType().AssemblyQualifiedName;
        }

        public override Object Resolve()
        {
            var go = (GameObject) base.Resolve();
            if (go == null)
            {
                PLog.Warn<MagnusLogger>($"Could not find an object at '{Path}'.");
                return null;
            }
            
            var type = ReflectionUtility.FindTypeExtensively(ref ComponentType);
            if (type == null)
            {
                PLog.Error<MagnusLogger>($"Could not resolve type '{ComponentType}'.");
                return null;
            }
            var comp = go.GetComponent(type);
            
            if (comp == null)
                PLog.Warn<MagnusLogger>($"Could not find '{type.Name}' on object at '{Path}'");
            return comp;
        }

        protected bool Equals(SceneComponentReferenceResolver other)
        {
            return base.Equals(other) && ComponentType == other.ComponentType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SceneComponentReferenceResolver) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (ComponentType != null ? ComponentType.GetHashCode() : 0);
            }
        }
    }
}