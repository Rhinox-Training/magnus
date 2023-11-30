using System;

namespace Rhinox.Magnus
{
    [Serializable]
    public abstract class BaseUnitySafeTypeFactory
    {
        public abstract object BuildGenericType(System.Type t, System.Type genericTypeDefinition);
    }
}