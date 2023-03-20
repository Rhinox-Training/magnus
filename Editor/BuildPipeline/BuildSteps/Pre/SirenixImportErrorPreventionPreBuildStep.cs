#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace Rhinox.Magnus.Editor
{
    public class SirenixImportErrorPreventionPreBuildStep : PreBuildStep
    {
        protected override bool OnExecute()
        {
            GlobalConfig<ImportSettingsConfig>.Instance.AutomateBeforeBuild = false;
            return true;
        }
    }
}
#endif