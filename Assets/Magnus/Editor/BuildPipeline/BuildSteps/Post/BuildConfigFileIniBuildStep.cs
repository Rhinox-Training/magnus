using System;
using System.IO;
using System.Reflection;
using Rhinox.Lightspeed.IO;
using Rhinox.Utilities;
using UnityEditor;

namespace Rhinox.Magnus.Editor
{
    public class BuildConfigFileIniBuildStep : PostBuildStep
    {
        public string OutputFolder;
        protected override bool OnExecute(BuildTarget target, string buildDirectory, string projectFileName)
        {
            if (OutputFolder == null)
                OutputFolder = "";
            var di = new DirectoryInfo(buildDirectory);

            foreach (Type configFileType in ConfigFileManager.GetConfigTypes())
            {
                IConfigFile cf = ConfigFileManager.GetConfig(configFileType);
                if (!(cf is ILoadableConfigFile loadableConfigFile))
                    continue;

                string filePath = Path.Combine(buildDirectory, OutputFolder, loadableConfigFile.RelativeFilePath);
                loadableConfigFile.Save(filePath, true);
            }

            return true;
        }

    }
}