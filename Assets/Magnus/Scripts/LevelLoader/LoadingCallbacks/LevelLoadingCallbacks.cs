using UnityEngine;

namespace Rhinox.Magnus
{
    public abstract class LevelLoadingCallbacks : MonoBehaviour
    {
        protected virtual void Awake()
        {
            
        }

        public abstract void HandleProgress(LoadingStage stage, int stageIndex, int totalStages, float progress,
            string name = null);
    }
}