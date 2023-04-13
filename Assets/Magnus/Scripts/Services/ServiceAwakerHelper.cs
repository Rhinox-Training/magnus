using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus
{
    internal class ServiceAwakerHelper : MonoBehaviour
    {
        private void OnDestroy()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex > 0 || !string.IsNullOrEmpty(activeScene.name))
                return;
            ServiceInitiator.AwakeServices();
            ServiceInitiator.UnloadService<InternalHelperService>();
        }
    }
}