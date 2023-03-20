using System;

namespace Rhinox.Magnus
{
    public class ServiceLoaderAttribute : Attribute
    {
        public int Order { get; }
        public ServiceLoaderAttribute(int order = 0)
        {
            Order = order;
        }
    }
}