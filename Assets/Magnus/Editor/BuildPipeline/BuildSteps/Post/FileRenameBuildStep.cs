using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Editor
{
    public class FileRenameBuildStep : PostBuildStep
    {
        public string SourceFilePath;
        public string TargetFilePath;

        protected override bool OnExecute(BuildTarget target, string buildDirectory, string projectFileName)
        {
            if (string.IsNullOrWhiteSpace(SourceFilePath) || string.IsNullOrWhiteSpace(TargetFilePath))
                return false;

            string targetPath = Path.Combine(buildDirectory, TargetFilePath);
            FileInfo targetFile = new FileInfo(targetPath);
            if (targetFile.Exists)
                File.Delete(targetPath);

            File.Move(Path.Combine(buildDirectory, SourceFilePath), targetPath);
            return true;
        }
    }
}