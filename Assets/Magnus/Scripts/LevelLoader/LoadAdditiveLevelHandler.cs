using System;
using System.Collections.Generic;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus
{
    public class LoadAdditiveLevelHandler : ILevelLoadHandler
    {
        public int LoadOrder => LevelLoadOrder.TASK_LOADING - 1;
        
        public string SceneName;
        
        public LoadAdditiveLevelHandler() {}

        public LoadAdditiveLevelHandler(string sceneName)
        {
            SceneName = sceneName;
        }

        public IEnumerator<float> OnLevelLoad()
        {
            yield return 0.0f;
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;

            PLog.Debug<MagnusLogger>($"Additive Level load triggered: {SceneName}");
            //When the load is still in progress, output the Text and progress bar
            while (!asyncOperation.isDone)
            {
                // Check if the load has finished
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }
                
                yield return asyncOperation.progress;
            }
            
            yield return 1.0f;
        }
    }
}