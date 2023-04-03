using UnityEngine;
using UnityEngine.UI;

namespace Rhinox.Magnus
{
    [RequireComponent(typeof(Slider))]
    public class SliderLoadingCallback : LevelLoadingCallbacks
    {
        private Slider _slider;

        protected override void Awake()
        {
            base.Awake();
            _slider = GetComponent<Slider>();
        }

        public override void HandleProgress(LoadingStage stage, int stageIndex, int totalStages, float progress, string name = null)
        {
            _slider.value = progress;
        }
    }
}