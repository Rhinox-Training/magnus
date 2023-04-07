using System;

namespace Rhinox.Magnus
{
    public class ServiceLoaderAttribute : Attribute
    {
        public int LoadOrder { get; }
        public ServiceLoaderAttribute(int loadOrder = 0)
        {
            LoadOrder = loadOrder;
        }
    }
}