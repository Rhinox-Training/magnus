using System;
using System.Collections;
using System.IO;
using System.Linq;
using Rhinox.Lightspeed.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Rhinox.Magnus.Editor
{
    public class EditorBuildTask : IBuildTask
    {
        private readonly BuildConfig _config;
        private UnityUserBuildSettingsData _backupSettings;
        private UnityUserBuildSettingsData _currentSettings;
        private bool _isBuilding;
#if UNITY_2021_2_OR_NEWER
        private static MethodInfo _activeSubTargetMethod;
#endif
        public int callbackOrder => -1;

        public EditorBuildTask(BuildConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _config = config;
        }


        public void RunPreBuild(BuildReport report)
        {
            Debug.Log("Pre-Build Phase Started");
            if (_config.PreBuildSteps == null || _config.PreBuildSteps.Count == 0)
            {
                Debug.Log("No pre build steps configured...");
                return;
            }

            for (var i = 0; i < _config.PreBuildSteps.Count; i++)
            {
                PreBuildStep step = _config.PreBuildSteps[i];
                Debug.Log($"-- Running step {i} ({step.GetType().Name})");
                step.Execute(report);
            }

            Debug.Log("Pre-Build Phase Ended");
        }

        public bool Run()
        {
            _backupSettings = UnityUserBuildSettingsData.CaptureCurrent();

            _currentSettings = UnityUserBuildSettingsData.Create(_config);
            _currentSettings.Apply();

            if (!TriggerBuild())
            {
                _backupSettings.Apply();
                _currentSettings = null;
                _backupSettings = null;
                return false;
            }
            _backupSettings.Apply();
            _currentSettings = null;
            _backupSettings = null;

            return true;
        }

        private bool TriggerBuild()
        {
            var scenes = _config.Scenes.Select(x => x.ScenePath).ToArray();
            BuildTarget buildTarget = _config.Platform.AsBuildTarget();
            BuildTargetGroup targetGroup = _config.Platform.AsBuildTargetGroup();
            var buildOptions = _config.GetBuildOptions();
            var path = MagnusUtils.GetBuildPathFromConfig(_config);

            return BuildProject(scenes, path, targetGroup, buildTarget, buildOptions);
        }

        private string EscapeSensitiveRegex(string str)
        {
            // +*?^$\.[]{}()|/ sensitive characters
            str = str.Replace("+", "\\+");
            str = str.Replace("*", "\\*");
            str = str.Replace("?", "\\?");
            str = str.Replace("^", "\\^");
            str = str.Replace("$", "\\$");
            str = str.Replace("\\", "\\\\");
            str = str.Replace(".", "\\.");
            str = str.Replace("[", "\\[");
            str = str.Replace("]", "\\]");
            str = str.Replace("{", "\\{");
            str = str.Replace("}", "\\}");
            str = str.Replace("(", "\\(");
            str = str.Replace(")", "\\)");
            str = str.Replace("|", "\\|");
            str = str.Replace("/", "\\/");
            return str;
        }

        private bool BuildProject(string[] scenes, string targetDir, BuildTargetGroup buildTargetGroup,
            BuildTarget buildTarget, BuildOptions buildOptions)
        {
            Debug.Log("Building:" + targetDir + " buildTargetGroup:" + buildTargetGroup.ToString() + " buildTarget:" +
                      buildTarget.ToString());

            // https://docs.unity3d.com/ScriptReference/EditorUserBuildSettings.SwitchActiveBuildTarget.html
            // bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
            // if (switchResult){
            //     System.Console.WriteLine("[JenkinsBuild] Successfully changed Build Target to: " + buildTarget.ToString());
            // }
            // else {
            //     System.Console.WriteLine("[JenkinsBuild] Unable to change Build Target to: " + buildTarget.ToString() + " Exiting...");
            //     return;
            // }

            // https://docs.unity3d.com/ScriptReference/BuildPipeline.BuildPlayer.html

            var options = new BuildPlayerOptions()
            {
                scenes = scenes,
                locationPathName = targetDir,
                targetGroup = buildTargetGroup,
                target = buildTarget,
                options = buildOptions,
#if UNITY_2021_2_OR_NEWER
                subtarget = FindSubTarget(buildTarget)
#endif
            };

            BuildReport buildReport = BuildPipeline.BuildPlayer(options);
            BuildSummary buildSummary = buildReport.summary;
            if (buildSummary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build Success: Time:{buildSummary.totalTime} Size:{buildSummary.totalSize} bytes");
                return true;
            }
            else
            {
                Debug.LogError($"Build Failed: Time:{buildSummary.totalTime} Total Errors:{buildSummary.totalErrors}");
                return false;
            }
        }

#if UNITY_2021_2_OR_NEWER
        private int FindSubTarget(BuildTarget buildTarget)
        {
            if (_config.Headless && buildTarget.IsStandalone())
                return (int)StandaloneBuildSubtarget.Server;

            if (_activeSubTargetMethod == null)
            {
                _activeSubTargetMethod = typeof(EditorUserBuildSettings).GetMethod("GetActiveSubtargetFor",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }
            
            return (int)_activeSubTargetMethod.Invoke(null, new object[] {buildTarget});
        }
#endif

        public IEnumerator RunPostBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (_config.PostBuildSteps == null || _config.PostBuildSteps.Count == 0)
            {
                Debug.Log("No post build steps configured...");
                yield break;
            }

            Debug.Log("Starting Post-Build Phase");
            for (var i = 0; i < _config.PostBuildSteps.Count; i++)
            {
                PostBuildStep step = _config.PostBuildSteps[i];
                Debug.Log($"-- Running step {i} ({step.GetType().Name})");
                string filePath = null;
                string buildDirectory = pathToBuiltProject;
                if (FileHelper.Exists(pathToBuiltProject))
                {
                    string rootDirectory = Path.GetDirectoryName(pathToBuiltProject);
                    var rootDI = new DirectoryInfo(rootDirectory);
                    buildDirectory = rootDI.FullName;
                    filePath = Path.GetFileName(pathToBuiltProject);
                }

                step.Execute(target, buildDirectory, filePath);
                yield return null;
            }

            Debug.Log("Post-Build Phase Ended");
            yield return new WaitForSeconds(2.0f);

            if (_config.PreBuildSteps != null || _config.PreBuildSteps.Count == 0)
            {
                foreach (var preBuildStep in _config.PreBuildSteps)
                {
                    if (preBuildStep == null)
                        continue;
                    preBuildStep.CleanUp();
                }
            }

            if (_backupSettings != null)
                _backupSettings.Apply(); // Restore
            _backupSettings = null;
        }
    }
}