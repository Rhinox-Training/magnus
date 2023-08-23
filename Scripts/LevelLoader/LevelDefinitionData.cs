using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable, RefactoringOldNamespace("", "com.rhinox.volt")]
public class LevelDefinitionData
{
    public int ID;

    public int Order; // This is only for order in UI list; should this even be here? TODO: No it shouldn't

    public string Name;
    public string DisplayName;
    
    public SceneReferenceData Scene;

    public SceneReferenceData[] AdditionalScenes;
    
    [ValueDropdown(nameof(GetGuidOptions))]
    public SerializableGuid PlayerStart;

    [ListDrawerSettings(Expanded = true)]
    public ILevelLoadHandler[] LoadHandlers;

    public void LoadLevel()
    {
        LevelLoader.Instance.LoadScene(this);
    }

    private ValueDropdownItem[] GetGuidOptions()
    {
        // TODO: is this ok?
        var assets = Resources.LoadAll<GuidAsset>("");
        return assets
            .Select(x => new ValueDropdownItem(x.PrettyName, x.GUID))
            .ToArray();
    }

    public GuidAsset GetPlayerStartIdentifier()
    {
        if (PlayerStart == null) return null;
        var result = GuidAsset.Find(PlayerStart);
        if (result == null)
            PLog.Error<MagnusLogger>($"PlayerStart was set on LevelData with ID '{ID}' but no GuidAsset could be found. [Guid={PlayerStart}]");
        return result;
    }
}
