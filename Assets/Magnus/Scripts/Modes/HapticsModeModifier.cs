using System.Collections.Generic;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.VOLT;

namespace Rhinox.VOLT.Domain
{
    public class HapticsModeModifier : GameModeModifier
    {
        public List<HapticsConfigTemplate> Templates;
        
        protected override void OnEnable()
        {
            if (Templates == null || Templates.Count == 0)
            {
                PLog.Warn<MagnusLogger>("No Haptics configured for this modeModifier, skipping...");
                return;
            }
            
            foreach (var template in Templates)
            {
                if (template == null)
                    continue;
                
                HapticsManager.Instance.Register(template, true); // TODO: this is destructive, what if default haptics profile
            }
        }

        protected override void OnDisable()
        {
            if (Templates == null || Templates.Count == 0)
            {
                PLog.Warn<MagnusLogger>("No Haptics configured for this modeModifier, skipping...");
                return;
            }
            
            HapticsManager.Instance.ClearConfig(); // TODO: too aggressive, should just deregister some values
        }
    }
}