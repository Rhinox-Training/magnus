using System;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Utilities;
using Rhinox.Utilities.Attributes;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Rhinox.Magnus
{
    [CustomProjectSettings(RuntimeSupported = true)]
    public class MagnusProjectSettings : CustomProjectSettings<MagnusProjectSettings>
    {
        [Title("Loading Scene"), AssetsOnly] 
        public LevelLoadingArea LoadingScenePrefab;
        [Title("Player System"), AssetsOnly]
        public PlayerConfig PlayerConfig;
        [Title("Game Modes"), AssetsOnly] 
        public GameModeConfig GameModes;
        [Title("Command System")]
        public string CommandSystemSecret;

        [Title("Services"), HideLabel] 
        public ServiceSettings ServiceSettings;

        private void OnEnable()
        {
            if (ServiceSettings == null)
                ServiceSettings = new ServiceSettings();

            ServiceSettings.Reinitialize();
        }
        
        protected override void LoadDefaults()
        {
            base.LoadDefaults();
#if UNITY_EDITOR
            if (LoadingScenePrefab == null)
                LoadingScenePrefab = Utility.FindAssetApproximately<LevelLoadingArea>("P_LoadingArea", "LoadingArea");
            
            if (PlayerConfig == null)
                PlayerConfig = Utility.FindAssetApproximately<PlayerConfig>("EmptyPlayerConfig");
#endif
        }
    }
}