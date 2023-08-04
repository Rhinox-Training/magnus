using UnityEngine.iOS;

namespace Rhinox.Magnus
{
    public abstract class HapticsDeviceHandler
    {
        public abstract bool HandleHaptics(DeviceHapticSpecifier specifier, HapticStrength strength);
    }
}