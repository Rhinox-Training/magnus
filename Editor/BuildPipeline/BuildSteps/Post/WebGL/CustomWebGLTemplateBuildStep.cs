using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.IO;
using Sirenix.OdinInspector;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Rhinox.Magnus.Editor
{
    public class CustomWebGLTemplateBuildStep : PostBuildStep
    {
        [ValueDropdown(nameof(GetTemplates))] 
        public string Template;

        public string IndexFileOverride = null;

        private const string WEBGLTEMPLATE_FOLDER_NAME = "WebGLTemplates";
        private const string STANDARD_UNITY_PACKAGE_PREFIX = "com.unity.";

        private string IndexFileName
        {
            get { return string.IsNullOrWhiteSpace(IndexFileOverride) ? "index.html" : IndexFileOverride; }
        }

        protected override bool OnExecute(BuildTarget target, string buildDirectory, string projectFileName)
        {
            if (target != BuildTarget.WebGL || string.IsNullOrWhiteSpace(Template))
                return false;

            //create template path
            string absolute = Path.GetFullPath(Template);

            //Clear the TemplateData folder, built by Unity.
            FileHelper.CreateOrCleanDirectory(FileHelper.Combine(buildDirectory, "TemplateData"));

            //Copy contents from WebGLTemplate. Ignore all .meta files
            FileHelper.CopyDirectoryFiltered(absolute, buildDirectory, true, @".*/\.+|\.meta$", true);

            //Replace contents of index.html
            FixIndexFile(buildDirectory);
            return true;
        }

        //Replaces %...% defines in index file (e.g. index.html)
        private void FixIndexFile(string pathToBuiltProject)
        {
            //Fetch filenames to be referenced in index.html
            string webglLoaderUrl;
            if (File.Exists(FileHelper.Combine(pathToBuiltProject, "Build", "UnityLoader.js")))
            {
                webglLoaderUrl = "Build/UnityLoader.js";
            }
            else
            {
                webglLoaderUrl = "Build/UnityLoader.min.js";
            }

            string buildName = pathToBuiltProject.Substring(pathToBuiltProject.LastIndexOf("/") + 1);
            var webglBuildUrl = $"Build/{buildName}.json";

            Dictionary<string, string> replaceKeywordsMap = new Dictionary<string, string>
            {
                {    "%UNITY_WIDTH%",                PlayerSettings.defaultWebScreenWidth.ToString() },
                {    "%UNITY_HEIGHT%",               PlayerSettings.defaultWebScreenHeight.ToString() },
                {    "%UNITY_WEB_NAME%",             PlayerSettings.productName },
                {    "%UNITY_WEBGL_LOADER_URL%",     webglLoaderUrl },
                {    "%UNITY_WEBGL_BUILD_URL%",      webglBuildUrl }
            };

            if (IndexFileName != "index.html")
            {
                string oldIndexFile = FileHelper.Combine(pathToBuiltProject, "index.html");
                if (File.Exists(oldIndexFile))
                    File.Delete(oldIndexFile);
            }
            
            string indexFilePath = FileHelper.Combine(pathToBuiltProject, IndexFileName);
            if (File.Exists(indexFilePath))
            {
                string fileContents = File.ReadAllText(indexFilePath);
                foreach (string keyword in replaceKeywordsMap.Keys)
                {
                    string replacement = replaceKeywordsMap[keyword];
                    fileContents = fileContents.Replace(keyword, replacement);
                }

                File.WriteAllText(indexFilePath, fileContents);
            }
        }
        
        private ICollection<ValueDropdownItem> GetTemplates()
        {
            List<ValueDropdownItem> result = new List<ValueDropdownItem>();

            foreach (var template in FindTemplatesAtPath(FileHelper.Combine("Assets", WEBGLTEMPLATE_FOLDER_NAME)))
                result.Add(new ValueDropdownItem(template, template));

            foreach (var path in FindPotentialPackageDirectoriesInAssets())
            {
                string templatesPath = FileHelper.Combine(path, WEBGLTEMPLATE_FOLDER_NAME);
                string absoluteTemplatesPath = Path.GetFullPath(templatesPath);
                if (!Directory.Exists(absoluteTemplatesPath))
                    continue;

                foreach (var template in FindTemplatesAtPath(absoluteTemplatesPath))
                {
                    string relativePath = FileHelper.GetRelativePath(template, FileHelper.GetProjectPath());
                    result.Add(new ValueDropdownItem(relativePath, relativePath));
                }
            }

            ListRequest lr = Client.List(true, false);
            while (!lr.IsCompleted)
            {
                // Wait
            }

            foreach (var package in lr.Result)
            {
                string packageName = package.name;
                if (packageName.StartsWith(STANDARD_UNITY_PACKAGE_PREFIX))
                    continue;

                string templatesPath = FileHelper.Combine("Packages", packageName, WEBGLTEMPLATE_FOLDER_NAME);
                string absoluteTemplatesPath = Path.GetFullPath(templatesPath);
                if (!Directory.Exists(absoluteTemplatesPath))
                    continue;

                foreach (var template in FindTemplatesAtPath(absoluteTemplatesPath))
                {
                    DirectoryInfo templateDI = new DirectoryInfo(template);
                    string relativePackageName = FileHelper.Combine(templatesPath, templateDI.Name);
                    result.Add(new ValueDropdownItem(relativePackageName, relativePackageName));
                }
            }

            return result;
        }

        private IEnumerable<string> FindPotentialPackageDirectoriesInAssets()
        {
            foreach (string filePath in Directory.EnumerateFiles(Path.GetFullPath("Assets"), "package.json",
                SearchOption.AllDirectories))
            {
                FileInfo f = new FileInfo(filePath);
                if (!f.Exists)
                    continue;
                yield return f.Directory.FullName;
            }
        }

        private IEnumerable<string> FindTemplatesAtPath(string templateSearchPath)
        {
            string absolutePath = Path.GetFullPath(templateSearchPath);
            if (!Directory.Exists(absolutePath))
                yield break;
            foreach (var templatePossibility in Directory.EnumerateDirectories(absolutePath))
            {
                if (!IsValidTemplate(templatePossibility))
                    continue;
                yield return templatePossibility;
            }
        }

        private bool IsValidTemplate(string templatePath)
        {
            string absolutePath = Path.GetFullPath(templatePath);
            if (!Directory.Exists(absolutePath))
                return false;

            return File.Exists(FileHelper.Combine(absolutePath, IndexFileName));
        }
    }
}