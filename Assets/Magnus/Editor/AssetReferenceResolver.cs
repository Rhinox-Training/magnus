using System;
using Object = UnityEngine.Object;
using UnityEditor;

namespace Rhinox.Magnus.Editor
{
    [Serializable]
    public class AssetReferenceResolver : IObjectReferenceResolver
    {
        public string AssetGuid;
        public string ObjectType;
        
        public string ErrorMessage => $"Guid not found: {AssetGuid} [{ObjectType}]";
        public string Description => $"Currently bound to: {AssetGuid} [{ObjectType}]";

        public AssetReferenceResolver(Object o) : this(null, o) { }

        public AssetReferenceResolver(string guid, Object o)
        {
            if (string.IsNullOrEmpty(guid))
                guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
            
            AssetGuid = guid;
            ObjectType = o.GetType().AssemblyQualifiedName;
        }

        public Object Resolve()
        {
            var path = AssetDatabase.GUIDToAssetPath(AssetGuid);
            var type = Type.GetType(ObjectType);
            return AssetDatabase.LoadAssetAtPath(path, type);
        }
        
        [ReferenceResolver(50, IsEditorOnly = true)]
        public static bool TryEncode(UnityEngine.Object target, out IObjectReferenceResolver resolver)
        {
            var path = AssetDatabase.GetAssetPath(target);
            
            resolver = null;
            if (string.IsNullOrWhiteSpace(path))
                return false;
            
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrWhiteSpace(guid))
                return false;

            resolver = new AssetReferenceResolver(guid, target);
            return true;
        }

        public bool Equals(IObjectReferenceResolver obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssetReferenceResolver) obj);
        }
        
        protected bool Equals(AssetReferenceResolver other)
        {
            return AssetGuid == other.AssetGuid && ObjectType == other.ObjectType;
        }
    }

}
