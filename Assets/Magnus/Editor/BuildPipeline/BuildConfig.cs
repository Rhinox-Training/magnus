using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Rhinox.GUIUtils.Editor;
using UnityEditor;
#endif
using UnityEngine;

namespace Rhinox.Magnus.Editor
{
    public enum BuildPlatform
    {
        Windows,
        Linux,
        Mac,
        Android,
        iOS,
        WebGL
    };

    public enum BuildConfiguration
    {
        Debug,
        DebugDeep,
        Release
    }

    public enum CompressionOptions
    {
        Default,
        None,
        Full
    }

    public class BuildConfig : ScriptableObject
    {
        [ValueDropdown("GetBuildPlatformsPretty"), HorizontalGroup("BuildConfig"), HideLabel]
        public BuildPlatform Platform;

        [HorizontalGroup("BuildConfig"), HideLabel]
        public BuildConfiguration Configuration;

        [ListDrawerSettings(Expanded = true, DraggableItems = true, CustomAddFunction = "OnAddScene")]
        [CustomValueDrawer("OnDrawScenes")]
        [CustomContextMenu("Import Scenes from Build Settings", nameof(UseScenesFromBuildSettings))]
        [SerializeReference]
        public SceneReferenceData[] Scenes;

        [PropertyTooltip("$OutputFormatTooltip")]
        [PropertyOrder(50)]
        public string OutputFormat = "{project}/{project}_{config}_{date}_{rc}";

        [FolderPath, PropertyOrder(51)]
        public string OutputFolder;

        [ReadOnly, PropertyOrder(51)]
        [LabelText("$FinalOutputLocation")]
        private string FinalOutputLocation => $"{OutputFolder}/{OutputFormat}";

        [FoldoutGroup("Advanced", order: 100)]
        [ListDrawerSettings(DraggableItems = true)]
        [SerializeReference]
        public List<PreBuildStep> PreBuildSteps = new List<PreBuildStep>();

        [FoldoutGroup("Advanced", order: 100)]
        [ListDrawerSettings(DraggableItems = true)]
        [SerializeReference]
        public List<PostBuildStep> PostBuildSteps = new List<PostBuildStep>();

        [FoldoutGroup("Advanced", order: 100)]
        public string ScriptingDefineSymbols;

        [FoldoutGroup("Advanced", order: 100)]
        public bool Headless = false;

        [FoldoutGroup("Advanced", order: 100)]
        [DisableIf(nameof(Configuration), BuildConfiguration.Release)]
        public bool WaitForPlayerConnection;

        [FoldoutGroup("Advanced", order: 100)]
        [DisableIf(nameof(Configuration), BuildConfiguration.Release)]
        public bool IncludeTestCode;

        [FoldoutGroup("Advanced", order: 100)]
        public CompressionOptions Compression;

        private void UseScenesFromBuildSettings()
        {
#if UNITY_EDITOR
            var scenes = Scenes != null ? Scenes.ToList() : new List<SceneReferenceData>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                bool existingScene =
                    scenes.Any(x => x.ScenePath.Equals(scene.path, StringComparison.InvariantCulture));
                if (existingScene)
                    continue;
                scenes.Add(new SceneReferenceData(scene.path));
            }
            Scenes = scenes.ToArray();
#endif
        }

        private string OutputFormatTooltip
        {
            get
            {
                string buildPath = MagnusUtils.GetBuildPathFromConfig(OutputFormat, OutputFolder,
                    name, Platform, Configuration);

                return "OutputFormat to use for building the project (supports subfolders):\n" +
                       "Available keywords: {project},{name},{config},{platform},{date},{time},{rc}\n" +
                       "e.g.: '{project}/{project}_{config}_{date}_{rc}'\n" +
                       "\n" +
                       $"Current: {buildPath}";
            }
        }

        #region ======= Drawer Code
#if UNITY_EDITOR
        // =============================================================================================================
        // Drawer Code
        public ICollection<ValueDropdownItem> GetBuildPlatformsPretty()
        {
            return Enum.GetValues(typeof(BuildPlatform))
                .OfType<BuildPlatform>()
                .Select(x => new ValueDropdownItem(x.ToString(), x))
                .ToArray();
        }

        private SceneReferenceData OnAddScene()
        {
            return new SceneReferenceData();
        }

        private SceneReferenceData OnDrawScenes(SceneReferenceData scene, GUIContent label)
        {
            EditorGUILayout.BeginVertical();
            using (new eUtility.IndentedLayout())
            {
                if (label != null)
                    EditorGUILayout.PrefixLabel(label);

                // Draw scene selector
                var asset = scene.SceneAsset;

                var newAsset = EditorGUILayout.ObjectField(asset, typeof(SceneAsset), false);

                if (newAsset != null && newAsset != asset)
                {
                    // Call Constructor taking a SceneAsset
                    scene = (SceneReferenceData)Activator.CreateInstance(typeof(SceneReferenceData),
                        new[] { newAsset });
                    asset = newAsset;
                }
                else if (newAsset == null && newAsset != asset)
                    scene.ScenePath = null;

                // End of scene selector
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            return scene;
        }
#endif
        #endregion
    }

    public static class BuildConfigExtensions
    {
        public static bool IsValidForCurrentEditorPlatformTarget(this BuildConfig config)
        {
            var currentTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildTarget = config.Platform.AsBuildTarget();
            return (buildTarget.IsStandalone() && currentTarget.IsStandalone()) ||
                   buildTarget == currentTarget;
        }

        public static bool IsStandalone(this BuildTarget platform)
        {
            switch (platform)
            {
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneWindows:
#if !UNITY_2019_3_OR_NEWER
                case BuildTarget.StandaloneLinux:
#endif
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux64:
                    return true;
                default:
                    return false;
            }
        }

        public static BuildTarget AsBuildTarget(this BuildPlatform platform, bool force32bit = false)
        {
            switch (platform)
            {
                case BuildPlatform.Windows:
                    return force32bit ? BuildTarget.StandaloneWindows : BuildTarget.StandaloneWindows64;
                case BuildPlatform.Linux:
                    return BuildTarget.StandaloneLinux64;
                case BuildPlatform.Mac:
                    return BuildTarget.StandaloneOSX;
                case BuildPlatform.Android:
                    return BuildTarget.Android;
                case BuildPlatform.iOS:
                    return BuildTarget.iOS;
                case BuildPlatform.WebGL:
                    return BuildTarget.WebGL;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }

        public static BuildTargetGroup AsBuildTargetGroup(this BuildPlatform platform)
        {
            switch (platform)
            {
                case BuildPlatform.Windows:
                case BuildPlatform.Linux:
                case BuildPlatform.Mac:
                    return BuildTargetGroup.Standalone;
                case BuildPlatform.Android:
                    return BuildTargetGroup.Android;
                case BuildPlatform.iOS:
                    return BuildTargetGroup.iOS;
                case BuildPlatform.WebGL:
                    return BuildTargetGroup.WebGL;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }

        public static BuildOptions GetBuildOptions(this BuildConfig config, bool showBuiltPlayer = false)
        {
            BuildOptions options = BuildOptions.None | BuildOptions.StrictMode;

            switch (config.Configuration)
            {
                case BuildConfiguration.Debug:
                    options |= BuildOptions.Development;
                    options |= BuildOptions.AllowDebugging;
                    break;
                case BuildConfiguration.DebugDeep:
                    options |= BuildOptions.Development;
                    options |= BuildOptions.AllowDebugging;
                    options |= BuildOptions.EnableDeepProfilingSupport;
                    break;
                case BuildConfiguration.Release:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (config.WaitForPlayerConnection)
                options |= BuildOptions.WaitForPlayerConnection;

#if !UNITY_2021_2_OR_NEWER
            if (config.Headless)
                options |= BuildOptions.EnableHeadlessMode;
#endif

            switch (config.Compression)
            {
                case CompressionOptions.Default:
                    options |= BuildOptions.CompressWithLz4;
                    break;
                case CompressionOptions.None:
                    options |= BuildOptions.UncompressedAssetBundle;
                    break;
                case CompressionOptions.Full:
                    options |= BuildOptions.CompressWithLz4HC;
                    break;
            }

            if (config.IncludeTestCode)
            {
                options |= BuildOptions.ForceEnableAssertions;
                options |= BuildOptions.IncludeTestAssemblies;
            }

            if (showBuiltPlayer)
                options |= BuildOptions.ShowBuiltPlayer;

            return options;
        }
    }
}