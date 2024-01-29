using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.Lightspeed;
using UnityEngine;


namespace Rhinox.Magnus.Editor
{
    public static class MagnusUtils
    {
        public static string GetBuildPathFromConfig(BuildConfig buildConfig)
        {
            return GetBuildPathFromConfig(buildConfig.OutputFormat, buildConfig.OutputFolder, buildConfig.name,
            buildConfig.Platform, buildConfig.Configuration);
        }

        public static string GetBuildPathFromConfig(string configStr, string outputStr, string configName,
            BuildPlatform platform,
            BuildConfiguration buildConfig)
        {
            string buildPath = $"{outputStr}/{configStr.Trim()}";

            buildPath = buildPath.ToLinuxSafePath();
            if (buildPath.EndsWith("/"))
                buildPath = buildPath.Substring(0, buildPath.Length - 1);

            var buildTime = DateTime.Now;
            buildPath = buildPath.Replace(BuildConstants.DATE_FORMAT_KEY,
                buildTime.ToString(BetterBuildSettings.DateFormat));
            buildPath = buildPath.Replace(BuildConstants.TIME_FORMAT_KEY,
                buildTime.ToString(BetterBuildSettings.TimeFormat));
            buildPath = buildPath.Replace(BuildConstants.CONFIG_FORMAT_KEY,
                buildConfig == BuildConfiguration.Release ? "rel" : "dbg");
            buildPath = buildPath.Replace(BuildConstants.PROJECT_FORMAT_KEY, Application.productName.MakePathSafe());
            buildPath = buildPath.Replace(BuildConstants.PLATFORM_FORMAT_KEY, platform.ToString());
            buildPath = buildPath.Replace(BuildConstants.BUILDCONFIG_NAME_FORMAT_KEY, configName.Trim());

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

        public static string FindReleaseCandidate(string referencePath)
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
            string absoluteBuildRootFolder =
                Path.GetFullPath(Path.Combine(Application.dataPath, "..", BuildConstants.BUILD_ROOT_FOLDER));
            absoluteBuildRootFolder = Path.GetFullPath(Path.Combine(absoluteBuildRootFolder, searchPath, ".."));

            DirectoryInfo currentDir = new DirectoryInfo(absoluteBuildRootFolder);
            // Get the last directory
            var searchString = Path.GetFileName(searchPath);

            if (!currentDir.Exists || searchString.IsNullOrEmpty())
                return DefaultRC;

            // Loop over all directories in the root directory and see if any match the rc named directory
            Regex r = new Regex(searchString.Replace(BuildConstants.RELEASE_CANDIDATE_FORMAT_KEY,
                $"({RC_IDENTIFIER}[0-9]+)"));

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
    }
}