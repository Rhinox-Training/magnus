using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus
{
    public static class UnitySafeActivator
    {
        private static List<BaseUnitySafeTypeFactory> _factories;
        
        public static object CreateInstance(Type type)
        {
            if (type.IsConstructedGenericType)
                return ConstructSafeType(type);
            return Activator.CreateInstance(type);
        }

        public static T CreateInstance<T>()
        {
            return CreateInstance<T>(typeof(T));
        }

        public static T CreateInstance<T>(Type type)
        {
            var instance = CreateInstance(type);
            return (T) instance;
        }

        private static object ConstructSafeType(Type type)
        {
            if (_factories == null)
            {
                _factories = new List<BaseUnitySafeTypeFactory>();
                if (MagnusProjectSettings.Instance.GenerationSettings != null)
                {
                    var factories = (ICollection<BaseUnitySafeTypeFactory>) MagnusProjectSettings.Instance.GenerationSettings
                        .TypeFactories ?? Array.Empty<BaseUnitySafeTypeFactory>();
                    _factories = factories.ToList();
                }
            }

            foreach (var factory in _factories)
            {
                var instance = factory.BuildGenericType(type.GetGenericArguments()[0], type.GetGenericTypeDefinition());
                if (instance != null)
                    return instance;
            }

            return null;
        }
    }
}