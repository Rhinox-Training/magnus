using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GuidAsset : ScriptableObject
{
    [SerializeField, HideInInspector]
    private SerializableGuid _serializedGuid;

    [DisplayAsString, ShowInInspector]
    public SerializableGuid GUID => _serializedGuid;
    
    private const string DefaultFolder = "Assets/Resources/Guids";
    
    [InlineButton("RenameToPretty", "R")]
    public string PrettyName;

    [TypeFilter("GetUnitySceneTypes")]
    public SerializableType ComponentType;
    
    public bool SupportMultiple;

    private void Awake()
    {
        if (_serializedGuid.IsNullOrEmpty())
            _serializedGuid = SerializableGuid.CreateNew();
    }

    public static GuidAsset Find(SerializableGuid guid)
    {
        // TODO: register them somewhere?
        var options = Resources.LoadAll<GuidAsset>("");
        return options.FirstOrDefault(x => !x.GUID.IsNullOrEmpty() && x.GUID.Equals(guid));
    }

#if UNITY_EDITOR
    public static GuidAsset CreateNew(string name = null)
    {
        var asset = ScriptableObject.CreateInstance<GuidAsset>();
        if (!AssetDatabase.IsValidFolder(DefaultFolder))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(DefaultFolder), Path.GetFileName(DefaultFolder));
        var path = Path.Combine(DefaultFolder, (name ?? "New") + ".asset");
        asset._serializedGuid = SerializableGuid.CreateNew();
        AssetDatabase.CreateAsset(asset, path);

        return asset;
    }
    
    public void RenamePretty(string name)
    {
        PrettyName = name;
        RenameToPretty();
    }
    
    private void RenameToPretty()
    {
        string assetPath =  AssetDatabase.GetAssetPath(GetInstanceID());
        AssetDatabase.RenameAsset(assetPath, $"[ID] {PrettyName}");
    }

    [Button]
    private void RegenerateGuid()
    {
        if (EditorUtility.DisplayDialog("Regenerate Guid?", "Are you sure you want to remake this guid? It may break certain connections.", "Yes", "No"))
        {
            Undo.RecordObject(this, "Regenerate GUID");
            _serializedGuid = SerializableGuid.CreateNew();
        }
    }
#endif
    private Type[] _unityTypes;
    private IEnumerable<Type> GetUnitySceneTypes()
    {
        if (_unityTypes == null)
        {
            var types = ReflectionUtility.GetTypesInheritingFrom(typeof(Component));
            types.Add(typeof(GameObject));

            _unityTypes = types.ToArray();
        }

        return _unityTypes;
    }
}