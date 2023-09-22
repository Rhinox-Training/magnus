using Rhinox.Lightspeed;
using Rhinox.Perceptor;
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

        #if USING_GRAPHY
        [Title("Graphy"), AssetsOnly]
        public GameObject GraphyPrefab;
        #endif
        
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
            
            #if USING_GRAPHY
            if (GraphyPrefab == null)
                GraphyPrefab = Utility.FindAssetApproximately<GameObject>("[Graphy]", "Graphy");
            #endif
#endif
        }

        public T GetPlayerConfig<T>() where T : PlayerConfig
        {
            if (PlayerConfig is T castConfig)
                return castConfig;
            PLog.Warn<MagnusLogger>($"PlayerConfig is {PlayerConfig?.GetType()?.Name ?? "<null>"}, not compatible with {typeof(T).Name}");
            return default(T);
        }
    }
}