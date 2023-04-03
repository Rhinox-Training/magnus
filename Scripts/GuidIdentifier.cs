using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class GuidIdentifier : MonoBehaviour
{
    public Type TargetType => TargetComponent == null ? typeof(GameObject) : TargetComponent.GetType();
    
    [InlineIconButton("Plus", "CreateNew")]
    public GuidAsset GuidAsset;

    [ValueDropdown(nameof(GetMyComponents))]
    public Behaviour TargetComponent;

    private void OnEnable()
    {
        if (_activeIdentifiers == null)
            _activeIdentifiers = new List<GuidIdentifier>();
        
        _activeIdentifiers.Add(this);
    }
    
    private void OnDisable()
    {
        _activeIdentifiers?.Remove(this);
    }

    // ================================================================================================================
    // STATIC
    private static List<GuidIdentifier> _activeIdentifiers;

    public static GuidIdentifier[] GetAll(GuidAsset asset)
    {
        return _activeIdentifiers.Where(x => x.GuidAsset == asset).ToArray();
    }

    public static GuidIdentifier GetFor(Object o)
    {
        if (o is GameObject go)
            return GetFor(go);
        if (o is Behaviour b)
            return GetFor(b);
        return null;
    }
    
    public static GuidIdentifier GetFor(Behaviour o)
    {
        if (o == null) return null;
        
        for (int i = 0; i < _activeIdentifiers.Count; ++i)
        {
            var identifier = _activeIdentifiers[i];
            if (identifier.TargetComponent == o)
                return identifier;
        }

        return null;
    }
    
    public static GuidIdentifier GetFor(GameObject o)
    {
        if (o == null) return null;
        
        for (int i = 0; i < _activeIdentifiers.Count; ++i)
        {
            var identifier = _activeIdentifiers[i];
            if (identifier.TargetComponent == null && identifier.gameObject == o)
                return identifier;
        }

        return null;
    }

    public static GuidIdentifier GetFor(GuidAsset asset)
    {
        for (int i = 0; i < _activeIdentifiers.Count; ++i)
        {
            var identifier = _activeIdentifiers[i];
            if (identifier.GuidAsset == asset)
                return identifier;
        }

        return null;
    }
    
    public static GuidIdentifier GetFor(GuidAsset asset, Type targetType)
    {
        for (int i = 0; i < _activeIdentifiers.Count; ++i)
        {
            var identifier = _activeIdentifiers[i];
            if (identifier.GuidAsset == asset && identifier.TargetType == targetType)
                return identifier;
        }

        return null;
    }

    private ValueDropdownItem[] GetMyComponents()
    {
        var components = GetComponents<Behaviour>();
        var dropdownItems = new ValueDropdownItem[components.Length];
        int i = 0;
        dropdownItems[i++] = new ValueDropdownItem("GameObject", null);
        foreach (var c in components)
        {
            if (c == this) continue;
            
            dropdownItems[i++] = new ValueDropdownItem(c.GetType().Name, c);
        }
            
        return dropdownItems;
    }

#if UNITY_EDITOR
    [OnInspectorGUI]
    private void Warn()
    {
        if (GuidAsset == null)
        {
            EditorGUILayout.HelpBox("Guid Asset not configured.", MessageType.Warning);
            return;
        }

        if (GuidAsset.SupportMultiple) return;
        
        var countUsage = _activeIdentifiers.Count(x => x.GuidAsset == GuidAsset);
        if (countUsage > 1)
        {
            GUIContentHelper.PushDisabled(true);
            EditorGUILayout.HelpBox("Multiple usages of a Guid Asset detected. Mark it as 'Support Multiple' if intended.", MessageType.Warning);
            foreach (var identifier in _activeIdentifiers)
            {
                if (identifier.GuidAsset != GuidAsset) 
                    continue;
                if (identifier == this) 
                    continue;
                EditorGUILayout.ObjectField(GUIContent.none, identifier, typeof(GuidIdentifier), true);
            }
            GUIContentHelper.PopDisabled();
        }
    }
    
    private void CreateNew()
    {
        var asset = GuidAsset.CreateNew();
        asset.RenamePretty(name);
        GuidAsset = asset;
    }
#endif
}