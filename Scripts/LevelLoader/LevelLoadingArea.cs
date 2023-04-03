using System;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;

namespace Rhinox.Magnus
{
    public struct Progress
    {
        public string Name;
        public float Time;
    }

    public enum LoadingStage
    {
        Initializing,
        LoadingStage,
        CleaningUp
    }

    public static class LoadingStageExtensions
    {
        public static string Print(this LoadingStage stage)
        {
            switch (stage)
            {
                case LoadingStage.CleaningUp:
                    return "Cleaning Up";
                default:
                    return stage.ToString();
            }
        }
    }
    
    public class LevelLoadingArea : MonoBehaviour
    {
        public ILevelExitTransitionEffect[] ExitTransitionEffects = null;
        public ILevelEnterTransitionEffect[] EnterTransitionEffects = null;
        
        private LevelLoadingCallbacks[] _levelLoadingCallbacks;

        protected void Awake()
        {
            if (ExitTransitionEffects == null)
                ExitTransitionEffects = Array.Empty<ILevelExitTransitionEffect>();
            if (EnterTransitionEffects == null)
                EnterTransitionEffects = Array.Empty<ILevelEnterTransitionEffect>();

            _levelLoadingCallbacks = GetComponentsInChildren<LevelLoadingCallbacks>();
        }

        public virtual void HandleProgress(LoadingStage stage, int stageIndex, int totalStages, float progress, string name = null)
        {
            PLog.Trace<MagnusLogger>($"[{nameof(LevelLoadingArea)}] HandleProgress ({stage.ToString()}) {stageIndex}/{totalStages} {progress} ({name})");
            foreach (var callback in _levelLoadingCallbacks)
            {
                if (callback == null)
                    continue;
                PLog.Trace<MagnusLogger>($"[{nameof(LevelLoadingArea)}] HandleProgress on target {callback.name}");
                callback.HandleProgress(stage, stageIndex, totalStages, progress, name);
            }
        }
    }
}