using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus
{
    [Serializable]
    public class ResourceReferenceResolver : IObjectReferenceResolver
    {
        public string Path;
        public Type Type;

        private const string RESOURCES_FOLDER = "Resources/";
        
        public string ErrorMessage => $"Resource not found: {Path} [{Type?.Name}]";
        public string Description  => $"Currently bound to: {Path} [{Type?.Name}]";

        public ResourceReferenceResolver(string resourcePath): this(resourcePath, typeof(Object))
        {
        }

        public ResourceReferenceResolver(string resourcePath, Type type)
        {
            Path = resourcePath;
            Type = type;
        }
        
        public Object Resolve()
        {
            return Resources.Load(Path, Type);
        }
        
        [ReferenceResolver(-5)]
        public static bool TryEncode(UnityEngine.Object target, out IObjectReferenceResolver resolver)
        {
#if UNITY_EDITOR
            var assetPath = AssetDatabase.GetAssetPath(target);
            var index = assetPath.LastIndexOf(RESOURCES_FOLDER, StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0)
            {
                var resourcesPath = assetPath.Substring(index + RESOURCES_FOLDER.Length);
                var folder = System.IO.Path.GetDirectoryName(resourcesPath);
                var file = System.IO.Path.GetFileNameWithoutExtension(resourcesPath);
                resourcesPath = System.IO.Path.Combine(folder, file);
                
                resolver = new ResourceReferenceResolver(resourcesPath, target.GetType());
                return true;
            }
#else
            // TODO: Any method of finding out the resource path for the given Object at buildtime?
#endif
            resolver = null;
            return false;
        }

        protected bool Equals(ResourceReferenceResolver other)
        {
            return Path == other.Path && Equals(Type, other.Type);
        }

        public bool Equals(IObjectReferenceResolver resolver) => Equals((object) resolver);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ResourceReferenceResolver) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Path != null ? Path.GetHashCode() : 0) * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            }
        }
    }
}