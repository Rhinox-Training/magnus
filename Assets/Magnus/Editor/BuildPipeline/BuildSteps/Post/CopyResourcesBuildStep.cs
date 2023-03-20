using System.Collections.Generic;
using System.IO;
using Rhinox.Lightspeed.IO;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Editor
{
    public class CopyResourcesBuildStep : PostBuildStep
    {
        public List<string> ResourceList;
        
        public string OutputDirectory;
        
        protected override bool OnExecute(BuildTarget target, string buildDirectory, string projectFileName)
        {
            if (ResourceList == null || string.IsNullOrWhiteSpace(OutputDirectory))
                return false;

            foreach (var resourcePath in ResourceList)
            {
                string fullPath = Path.Combine(FileHelper.GetProjectPath(), resourcePath);
                var fi = new FileInfo(fullPath);
                if (!fi.Exists)
                {
                    Debug.LogWarning($"Failed to copy {fi.FullName}, not a file or does not exist...");
                    continue;
                }

                string fileName = fi.Name;
                string outputTargetPath = Path.Combine(buildDirectory, OutputDirectory, fileName);
                File.Copy(fullPath, outputTargetPath);
            }
            return true;
        }
    }
}