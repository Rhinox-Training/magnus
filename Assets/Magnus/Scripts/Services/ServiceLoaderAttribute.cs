using System;

namespace Rhinox.Magnus
{
    public class ServiceLoaderAttribute : Attribute
    {
        public int LoadOrder { get; }

        public bool DisabledByDefault = false;
        
        public ServiceLoaderAttribute(int loadOrder = 0, bool disabledByDefault = false)
        {
            LoadOrder = loadOrder;
            DisabledByDefault = disabledByDefault;
        }
    }
}