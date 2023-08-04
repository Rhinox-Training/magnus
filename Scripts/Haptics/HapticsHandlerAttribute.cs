using System;

namespace Rhinox.Magnus
{
    public class HapticsHandlerAttribute : Attribute
    {
        public Type DeviceType { get; }
        
        public HapticsHandlerAttribute(Type deviceHapticSpecifierType)
        {
            DeviceType = deviceHapticSpecifierType;
        }
    }
}