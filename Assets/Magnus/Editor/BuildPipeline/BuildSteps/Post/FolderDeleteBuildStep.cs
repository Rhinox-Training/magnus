using Rhinox.Lightspeed;
using Rhinox.Lightspeed.IO;
using UnityEditor;

namespace Rhinox.Magnus.Editor
{
    public class FolderDeleteBuildStep : PostBuildStep
    {
        public string TargetPath;
    
        protected override bool OnExecute(BuildTarget target, string buildDirectory, string projectFileName)
        {
            if (string.IsNullOrWhiteSpace(TargetPath))
                return false;

            string targetPath = FileHelper.GetFullFilePath(TargetPath, buildDirectory);
            FileHelper.DeleteDirectoryIfExists(targetPath);
            return true;
        }
    }
}