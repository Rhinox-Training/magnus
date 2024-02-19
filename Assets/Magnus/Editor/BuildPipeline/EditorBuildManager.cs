using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.Utilities.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Rhinox.Magnus.Editor
{
    public class PreBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -10000; // NOTE: this should be early enough to run first, (Odin e.g., uses -1500)
        public void OnPreprocessBuild(BuildReport report)
        {
            EditorBuildManager.Instance.NotifyPreprocess(report);
        }
    }

    public class EditorBuildManager
    {
        private static EditorBuildManager _instance;
        private IBuildTask _currentTask;

        public static EditorBuildManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EditorBuildManager();
                return _instance;
            }
        }

        public bool IsRunning => _currentTask != null;
        
        private const int POST_BUILD_DELAY_MS = 3000; // Milliseconds

        private EditorBuildManager()
        {
            
        }

        public static void TriggerBuildSmart(string searchString)
        {
            var config = FindConfigForRequest(searchString);
            if (config == null)
            {
                PLog.Warn<MagnusLogger>($"Request to build failed, Config not found for '{searchString}'. Stopping build...");
                return;
            }

            TriggerBuild(new EditorBuildTask(config));
        }

        private static BuildConfig FindConfigForRequest(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                PLog.Trace<MagnusLogger>($"Request to build was empty. Stopping build...");
                return null;
            }
            
            searchString = searchString.Trim();

            AssetDatabase.Refresh();
            var configs = Utility.FindAssets<BuildConfig>();
            foreach (var config in configs)
            {
                if (config.name.Equals(searchString, StringComparison.InvariantCulture))
                    return config;
            }

            return null;
        }

        public static bool TriggerBuild(IBuildTask task)
        {
            if (Instance.IsRunning)
            {
                Debug.LogError("Cannot trigger build when old build is still running");
                return false;
            }

            Instance._currentTask = task;
            try
            {
                if (!Instance._currentTask.Run())
                {
                    Debug.LogError("Run of build failed, exiting...");
                    Instance._currentTask = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Run of build failed {e.ToString()}, exiting...");
                Instance._currentTask = null;
                return false;
            }

            return true;
        }

        [PostProcessBuild(1)]
        private static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (Instance._currentTask == null) // A normal build is running do nothing
                return;
            
            Task.Delay(POST_BUILD_DELAY_MS)
                .ContinueWith(t => EditorDispatcher.Dispatch(Instance._currentTask.RunPostBuild(target, pathToBuiltProject)))
                .ContinueWith(t => EditorDispatcher.Dispatch(Instance.FinishBuild()));

        }

        private IEnumerator FinishBuild()
        {
            _currentTask = null;
            Debug.Log("Build completed!");
            yield break;
        }

        public void NotifyPreprocess(BuildReport report)
        {
            if (Instance._currentTask == null) // A normal build is running do nothing
                return;

            Instance._currentTask.RunPreBuild(report);
        }
    }
}