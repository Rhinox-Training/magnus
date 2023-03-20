using UnityEditor;

namespace Rhinox.Magnus.Editor
{
    public class QuestXRCalibrationBuildStep: PreBuildStep
    {
        private StereoRenderingPath _cache;

        protected override bool OnExecute()
        {
            _cache = PlayerSettings.stereoRenderingPath;
            PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
            return true;
        }

        protected override void OnCleanUp()
        {
            PlayerSettings.stereoRenderingPath = _cache;
            base.OnCleanUp();
        }
    }
}