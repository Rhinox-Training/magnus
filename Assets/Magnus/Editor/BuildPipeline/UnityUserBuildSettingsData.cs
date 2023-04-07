using System;
using UnityEditor;

namespace Rhinox.Magnus.Editor
{
    public class UnityUserBuildSettingsData
    {
        public BuildTarget activeBuildTarget;
        public string activeScriptCompilationDefines;
        public bool allowDebugging;
        public MobileTextureSubtarget androidBuildSubtarget;
#if !UNITY_2021_2_OR_NEWER
        public bool androidCreateSymbolsZip;
#else
        public AndroidCreateSymbols androidCreateSymbolsZip;
#endif
        public AndroidETC2Fallback androidETC2Fallback;
        public bool buildAppBundle;
        public bool buildScriptsOnly;
        public bool buildWithDeepProfilingSupport;
        public bool compressFilesInPackage;
#if !UNITY_2021_2_OR_NEWER
        public bool compressWithPsArc;
#endif
        public bool connectProfiler;
        public bool development;
#if !UNITY_2021_2_OR_NEWER
        public bool enableHeadlessMode;
#else
        public StandaloneBuildSubtarget standaloneBuildSubtarget;
#endif
        public bool explicitArrayBoundsChecks;
        public bool explicitDivideByZeroChecks;
        public bool explicitNullChecks;
        public bool exportAsGoogleAndroidProject;
        public bool forceInstallation;
        public bool installInBuildFolder;
#if UNITY_2021_1_OR_NEWER
        public XcodeBuildConfig iOSBuildConfigType;
#else
        public iOSBuildType iOSBuildConfigType;
#endif
        public bool movePackageToDiscOuterEdge;
        public bool needSubmissionMaterials;
        public BuildTargetGroup selectedBuildTargetGroup;
        public BuildTarget selectedStandaloneTarget;
        public int streamingInstallLaunchRange;
#if !UNITY_2021_2_OR_NEWER
        public bool symlinkLibraries;
#else
        public bool symlinkSources;
#endif
        public bool waitForManagedDebugger;
        public bool waitForPlayerConnection;
        public string buildLocation;

        public static UnityUserBuildSettingsData CaptureCurrent()
        {
            var data = new UnityUserBuildSettingsData();
            data.activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            data.activeScriptCompilationDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            data.allowDebugging = EditorUserBuildSettings.allowDebugging;
            data.androidBuildSubtarget = EditorUserBuildSettings.androidBuildSubtarget;
#if !UNITY_2021_2_OR_NEWER
            data.androidCreateSymbolsZip = EditorUserBuildSettings.androidCreateSymbolsZip;
#else
            data.androidCreateSymbolsZip = EditorUserBuildSettings.androidCreateSymbols;
#endif
            data.androidETC2Fallback = EditorUserBuildSettings.androidETC2Fallback;
            data.buildAppBundle = EditorUserBuildSettings.buildAppBundle;
            data.buildScriptsOnly = EditorUserBuildSettings.buildScriptsOnly;
            data.buildWithDeepProfilingSupport = EditorUserBuildSettings.buildWithDeepProfilingSupport;
            data.compressFilesInPackage = EditorUserBuildSettings.compressFilesInPackage;
#if !UNITY_2021_2_OR_NEWER
            data.compressWithPsArc = EditorUserBuildSettings.compressWithPsArc;
#endif
            data.connectProfiler = EditorUserBuildSettings.connectProfiler;
            data.development = EditorUserBuildSettings.development;
#if !UNITY_2021_2_OR_NEWER
            data.enableHeadlessMode = EditorUserBuildSettings.enableHeadlessMode;
#else
            data.standaloneBuildSubtarget = EditorUserBuildSettings.standaloneBuildSubtarget;
#endif
            data.explicitArrayBoundsChecks = EditorUserBuildSettings.explicitArrayBoundsChecks;
            data.explicitDivideByZeroChecks = EditorUserBuildSettings.explicitDivideByZeroChecks;
            data.explicitNullChecks = EditorUserBuildSettings.explicitNullChecks;
            data.exportAsGoogleAndroidProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            data.forceInstallation = EditorUserBuildSettings.forceInstallation;
            data.installInBuildFolder = EditorUserBuildSettings.installInBuildFolder;
#if UNITY_2021_1_OR_NEWER
            data.iOSBuildConfigType = EditorUserBuildSettings.iOSXcodeBuildConfig;
#else
            data.iOSBuildConfigType = EditorUserBuildSettings.iOSBuildConfigType;
#endif
            data.movePackageToDiscOuterEdge = EditorUserBuildSettings.movePackageToDiscOuterEdge;
            data.needSubmissionMaterials = EditorUserBuildSettings.needSubmissionMaterials;
            // data.ps4BuildSubtarget = EditorUserBuildSettings.ps4BuildSubtarget;
            // data.ps4HardwareTarget = EditorUserBuildSettings.ps4HardwareTarget;
            data.selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            data.selectedStandaloneTarget = EditorUserBuildSettings.selectedStandaloneTarget;
            data.streamingInstallLaunchRange = EditorUserBuildSettings.streamingInstallLaunchRange;
#if !UNITY_2021_1_OR_NEWER
            data.symlinkLibraries = EditorUserBuildSettings.symlinkLibraries;
#else
            data.symlinkSources = EditorUserBuildSettings.symlinkSources;
#endif
            data.waitForManagedDebugger = EditorUserBuildSettings.waitForManagedDebugger;
            data.waitForPlayerConnection = EditorUserBuildSettings.waitForPlayerConnection;
            // data.windowsDevicePortalAddress = EditorUserBuildSettings.windowsDevicePortalAddress;
            // data.windowsDevicePortalPassword = EditorUserBuildSettings.windowsDevicePortalPassword;
            // data.windowsDevicePortalUsername = EditorUserBuildSettings.windowsDevicePortalUsername;
            // data.wsaBuildAndRunDeployTarget = EditorUserBuildSettings.wsaBuildAndRunDeployTarget;
            // data.wsaSubtarget = EditorUserBuildSettings.wsaSubtarget;
            // data.wsaUWPBuildType = EditorUserBuildSettings.wsaUWPBuildType;
            // data.wsaUWPSDK = EditorUserBuildSettings.wsaUWPSDK;
            // data.wsaUWPVisualStudioVersion = EditorUserBuildSettings.wsaUWPVisualStudioVersion;
            // data.xboxBuildSubtarget = EditorUserBuildSettings.xboxBuildSubtarget;
            // data.xboxOneDeployDrive = EditorUserBuildSettings.xboxOneDeployDrive;
            // data.xboxOneDeployMethod = EditorUserBuildSettings.xboxOneDeployMethod;
            // data.xboxOneRebootIfDeployFailsAndRetry = EditorUserBuildSettings.xboxOneRebootIfDeployFailsAndRetry;
            data.buildLocation = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);
            return data;
        }

        public static UnityUserBuildSettingsData Create(BuildConfig config)
        {
            var current = UnityUserBuildSettingsData.CaptureCurrent();

            current.selectedStandaloneTarget = config.Platform.AsBuildTarget();
            switch (config.Configuration)
            {
                case BuildConfiguration.Debug:
                    current.development = true;
                    current.allowDebugging = true;
                    current.waitForManagedDebugger = false;
                    break;
                case BuildConfiguration.DebugDeep:
                    current.development = true;
                    current.allowDebugging = true;
                    current.waitForManagedDebugger = true;
                    break;
                case BuildConfiguration.Release:
                    current.development = false;
                    current.allowDebugging = false;
                    current.waitForManagedDebugger = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return current;
        }

        public void Apply()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(activeBuildTarget), activeBuildTarget);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(selectedBuildTargetGroup, activeScriptCompilationDefines);
            EditorUserBuildSettings.allowDebugging = allowDebugging;
            EditorUserBuildSettings.androidBuildSubtarget = androidBuildSubtarget;
#if !UNITY_2021_2_OR_NEWER
            EditorUserBuildSettings.androidCreateSymbolsZip = androidCreateSymbolsZip;
#else
            EditorUserBuildSettings.androidCreateSymbols = androidCreateSymbolsZip;
#endif
            EditorUserBuildSettings.androidETC2Fallback = androidETC2Fallback;
            EditorUserBuildSettings.buildAppBundle = buildAppBundle;
            EditorUserBuildSettings.buildScriptsOnly = buildScriptsOnly;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = buildWithDeepProfilingSupport;
            EditorUserBuildSettings.compressFilesInPackage = compressFilesInPackage;
#if !UNITY_2021_2_OR_NEWER
            EditorUserBuildSettings.compressWithPsArc = compressWithPsArc;
#endif
            EditorUserBuildSettings.connectProfiler = connectProfiler;
            EditorUserBuildSettings.development = development;
#if !UNITY_2021_1_OR_NEWER
            EditorUserBuildSettings.enableHeadlessMode = enableHeadlessMode;
#else
            EditorUserBuildSettings.standaloneBuildSubtarget = standaloneBuildSubtarget;
#endif
            EditorUserBuildSettings.explicitArrayBoundsChecks = explicitArrayBoundsChecks;
            EditorUserBuildSettings.explicitDivideByZeroChecks = explicitDivideByZeroChecks;
            EditorUserBuildSettings.explicitNullChecks = explicitNullChecks;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = exportAsGoogleAndroidProject;
            EditorUserBuildSettings.forceInstallation = forceInstallation;
            EditorUserBuildSettings.installInBuildFolder = installInBuildFolder;
#if UNITY_2021_1_OR_NEWER
            EditorUserBuildSettings.iOSXcodeBuildConfig = iOSBuildConfigType;
#else
            EditorUserBuildSettings.iOSBuildConfigType = iOSBuildConfigType;
#endif

            EditorUserBuildSettings.movePackageToDiscOuterEdge = movePackageToDiscOuterEdge;
            EditorUserBuildSettings.needSubmissionMaterials = needSubmissionMaterials;
            // EditorUserBuildSettings.ps4BuildSubtarget = ps4BuildSubtarget;
            // EditorUserBuildSettings.ps4HardwareTarget = ps4HardwareTarget;
            EditorUserBuildSettings.selectedBuildTargetGroup = selectedBuildTargetGroup;
            EditorUserBuildSettings.selectedStandaloneTarget = selectedStandaloneTarget;
            EditorUserBuildSettings.streamingInstallLaunchRange = streamingInstallLaunchRange;
#if !UNITY_2021_1_OR_NEWER
            EditorUserBuildSettings.symlinkLibraries = symlinkLibraries;
#else
            EditorUserBuildSettings.symlinkSources = symlinkSources;
#endif
            EditorUserBuildSettings.waitForManagedDebugger = waitForManagedDebugger;
            EditorUserBuildSettings.waitForPlayerConnection = waitForPlayerConnection;
            // EditorUserBuildSettings.windowsDevicePortalAddress = windowsDevicePortalAddress;
            // EditorUserBuildSettings.windowsDevicePortalPassword = windowsDevicePortalPassword;
            // EditorUserBuildSettings.windowsDevicePortalUsername = windowsDevicePortalUsername;
            // EditorUserBuildSettings.wsaBuildAndRunDeployTarget = wsaBuildAndRunDeployTarget;
            // EditorUserBuildSettings.wsaSubtarget = wsaSubtarget;
            // EditorUserBuildSettings.wsaUWPBuildType = wsaUWPBuildType;
            // EditorUserBuildSettings.wsaUWPSDK = wsaUWPSDK;
            // EditorUserBuildSettings.wsaUWPVisualStudioVersion = wsaUWPVisualStudioVersion;
            // EditorUserBuildSettings.xboxBuildSubtarget = xboxBuildSubtarget;
            // EditorUserBuildSettings.xboxOneDeployDrive = xboxOneDeployDrive;
            // EditorUserBuildSettings.xboxOneDeployMethod = xboxOneDeployMethod;
            // EditorUserBuildSettings.xboxOneRebootIfDeployFailsAndRetry = xboxOneRebootIfDeployFailsAndRetry;
            EditorUserBuildSettings.SetBuildLocation(EditorUserBuildSettings.activeBuildTarget, buildLocation);
        }
    }
}