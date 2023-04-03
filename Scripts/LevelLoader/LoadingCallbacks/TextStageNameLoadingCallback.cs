using TMPro;
using UnityEngine;

namespace Rhinox.Magnus
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextStageNameLoadingCallback : LevelLoadingCallbacks
    {
        private TextMeshProUGUI _text;

        protected override void Awake()
        {
            base.Awake();
            _text = GetComponent<TextMeshProUGUI>();
        }

        public override void HandleProgress(LoadingStage stage, int stageIndex, int totalStages, float progress, string name = null)
        {
            _text.text = name != null ? $"Loading {name}..." : $"{stage.Print()}...";
        }
    }
}