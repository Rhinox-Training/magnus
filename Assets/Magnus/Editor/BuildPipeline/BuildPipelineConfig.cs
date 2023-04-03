using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Rhinox.Lightspeed.IO;
using Rhinox.Utilities;
using Rhinox.Utilities.Editor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;

namespace Rhinox.Magnus.Editor
{
    [Obsolete] // TODO: can this be removed?
    public class BuildPipelineConfig : CustomProjectSettings<BuildPipelineConfig>, IPreprocessBuildWithReport
    {
        [ListDrawerSettings(DraggableItems = true)]
        public List<PreBuildStep> PreBuildSteps = new List<PreBuildStep>();
        
        [ListDrawerSettings(DraggableItems = true)]
        public List<PostBuildStep> PostBuildSteps = new List<PostBuildStep>();
    
        public int callbackOrder => -1;
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Pre-Build Phase Started");
            if (Instance.PreBuildSteps == null || Instance.PreBuildSteps.Count == 0)
            {
                Debug.Log("No pre build steps configured...");
                return;
            }

            for (var i = 0; i < Instance.PreBuildSteps.Count; i++)
            {
                PreBuildStep step = Instance.PreBuildSteps[i];
                Debug.Log($"-- Running step {i} ({step.GetType().Name})");
                step.Execute(report);
            }

            Debug.Log("Pre-Build Phase Ended");
        }
        
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            Task.Delay(3000).ContinueWith(t=> 
                EditorDispatcher.Dispatch(ExecuteBuildSteps(target, pathToBuiltProject)));
        }

        private static IEnumerator ExecuteBuildSteps(BuildTarget target, string pathToBuiltProject)
        {
            if (Instance.PostBuildSteps == null || Instance.PostBuildSteps.Count == 0)
            {
                Debug.Log("No post build steps configured...");
                yield break;
            }

            Debug.Log("Starting Post-Build Phase");
            for (var i = 0; i < Instance.PostBuildSteps.Count; i++)
            {
                PostBuildStep step = Instance.PostBuildSteps[i];
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
        }

        protected override void LoadDefaults()
        {
            PostBuildSteps = new List<PostBuildStep>();
        }
    }
}