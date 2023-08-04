namespace Rhinox.Magnus
{
    public abstract class HapticsSource
    {
        public abstract void Init();
        public abstract void Terminate();
        
        protected void TriggerHaptics(DeviceHapticSpecifier specifier)
        {
            HapticsManager.Instance.ExecuteHaptics(specifier, this);
        }
    }
}