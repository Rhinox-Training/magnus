using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.Lightspeed;
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
            var path = GetBuildPath(_config.OutputFormat, _config.Platform);
            return BuildProject(scenes, path, targetGroup, buildTarget, buildOptions);
        }

        private string GetBuildPath(string configStr, BuildPlatform platform)
        {
            string buildPath = configStr.Trim();

            buildPath = buildPath.ToLinuxSafePath();
            if (buildPath.EndsWith("/"))
                buildPath = buildPath.Substring(0, buildPath.Length - 1);

            var buildTime = DateTime.Now;
            buildPath = buildPath.Replace(BuildConstants.DATE_FORMAT_KEY, buildTime.ToString(BetterBuildSettings.DateFormat));
            buildPath = buildPath.Replace(BuildConstants.TIME_FORMAT_KEY, buildTime.ToString(BetterBuildSettings.TimeFormat));
            buildPath = buildPath.Replace(BuildConstants.CONFIG_FORMAT_KEY, _config.Configuration == BuildConfiguration.Release ? "rel" : "dbg");
            buildPath = buildPath.Replace(BuildConstants.PROJECT_FORMAT_KEY, MakePathSafe(Application.productName));
            buildPath = buildPath.Replace(BuildConstants.PLATFORM_FORMAT_KEY, _config.Platform.ToString());
            buildPath = buildPath.Replace(BuildConstants.BUILDCONFIG_NAME_FORMAT_KEY, _config.name.Trim());

            string currentReleaseCandidate = FindReleaseCandidate(buildPath);
            buildPath = buildPath.Replace(BuildConstants.RELEASE_CANDIDATE_FORMAT_KEY, currentReleaseCandidate);

            var combinedPath = Path.Combine(Application.dataPath, "..", BuildConstants.BUILD_ROOT_FOLDER, buildPath);

            var absPath = Path.GetFullPath(combinedPath);
            absPath = absPath.ToLinuxSafePath();

            switch (platform)
            {
                case BuildPlatform.Android:
                    if (absPath.EndsWith("/"))
                        absPath = absPath.Substring(0, absPath.Length - 1);
                    absPath += ".apk";
                    break;
                case BuildPlatform.Windows:
                    if (absPath.EndsWith("/"))
                        absPath = absPath.Substring(0, absPath.Length - 1);
                    absPath += ".exe";
                    break;
            }
            return absPath;
        }

        private string FindReleaseCandidate(string referencePath)
        {            
            const string RC_IDENTIFIER = "rc";
            const string DefaultRC = RC_IDENTIFIER + "1";
            
            referencePath = referencePath.Trim();
            if (string.IsNullOrWhiteSpace(referencePath))
                return DefaultRC;
            
            // Split into folder and filename
            int index = referencePath.LastIndexOf("/", StringComparison.InvariantCulture);
            
            string containingPath = "";
            string outputName = referencePath;
            if (index != -1)
            {
                containingPath = referencePath.Substring(0, index);
                outputName = referencePath.Substring(index + 1);
            }
            
            // Find the part that contains the RC key
            string searchPath;
            bool isDirectory = true;
            if (containingPath.Contains(BuildConstants.RELEASE_CANDIDATE_FORMAT_KEY))
            {
                // Take parts until you come across the first RC usage
                var subPaths = containingPath.Split('/');
                int resultIndex = -1;
                for (int i = 0; i < subPaths.Length; ++i)
                {
                    var path = subPaths[i];
                    if (!path.Contains(BuildConstants.RELEASE_CANDIDATE_FORMAT_KEY))
                        continue;
                    resultIndex = i;
                    break;
                }

                if (resultIndex != -1)
                    containingPath = string.Join("/", subPaths.Take(resultIndex + 1).ToArray());

                searchPath = containingPath;
            }
            else if (outputName.Contains(BuildConstants.RELEASE_CANDIDATE_FORMAT_KEY))
            {
                searchPath = referencePath;                
                isDirectory = false;
            }
            else
            {                
                return DefaultRC;
            }

            // Get the part of the path that is constant (aka no RC key)
            string absoluteBuildRootFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", BuildConstants.BUILD_ROOT_FOLDER));
            absoluteBuildRootFolder = Path.GetFullPath(Path.Combine(absoluteBuildRootFolder, searchPath, ".."));

            DirectoryInfo currentDir = new DirectoryInfo(absoluteBuildRootFolder);
            // Get the last directory
            var searchString = Path.GetFileName(searchPath);

            if (!currentDir.Exists || searchString.IsNullOrEmpty())
                return DefaultRC;

            // Loop over all directories in the root directory and see if any match the rc named directory
            Regex r = new Regex(searchString.Replace(BuildConstants.RELEASE_CANDIDATE_FORMAT_KEY, $"({RC_IDENTIFIER}[0-9]+)"));

            IEnumerable<FileSystemInfo> infos;
            if (isDirectory)
                infos = currentDir.EnumerateDirectories();
            else
                infos = currentDir.EnumerateFiles();

            var highestRc = infos
                .Select(x => r.Match(x.Name))
                .Where(x => x.Success)
                .Select(x => x.Groups[1].Value)
                .OrderByDescending(x => x)
                .FirstOrDefault();
            
            if (!string.IsNullOrWhiteSpace(highestRc))
            {
                string revStr = highestRc.Trim().Replace(RC_IDENTIFIER, "");
                if (int.TryParse(revStr, out int revision))
                    return $"{RC_IDENTIFIER}{revision + 1}";
            }
            
            return DefaultRC;
        }

        // TODO: Migrate to lightspeed
        private string MakePathSafe(string str)
        {
            var parts = str.Split(' ', '\n', '\r', '\t', '$', '^', '&', '|', ',')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
            return string.Join("_", parts);
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
        
        private static bool BuildProject(string[] scenes, string targetDir, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, BuildOptions buildOptions){
            Debug.Log("Building:" + targetDir + " buildTargetGroup:" + buildTargetGroup.ToString() + " buildTarget:" + buildTarget.ToString());

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
            BuildReport buildReport = BuildPipeline.BuildPlayer(scenes, targetDir, buildTarget, buildOptions);
            BuildSummary buildSummary = buildReport.summary;
            if (buildSummary.result == BuildResult.Succeeded){
                Debug.Log("Build Success: Time:" + buildSummary.totalTime + " Size:" + buildSummary.totalSize + " bytes");
                return true;
            }
            else {
                Debug.LogError("Build Failed: Time:" + buildSummary.totalTime + " Total Errors:" + buildSummary.totalErrors);
                return false;
            }
        }

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