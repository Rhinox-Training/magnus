using System;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus
{
    public interface IObjectReferenceResolver : IEquatable<IObjectReferenceResolver>
    {
        string ErrorMessage { get; }
        
        string Description { get; }
        
        Object Resolve();
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ReferenceResolverAttribute : Attribute
    {
        public int Order;
        public bool IsEditorOnly;

        public ReferenceResolverAttribute(int order = 0)
        {
            Order = order;
        }
    }
}