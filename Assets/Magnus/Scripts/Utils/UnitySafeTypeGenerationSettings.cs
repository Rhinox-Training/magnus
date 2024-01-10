using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
#if UNITY_EDITOR
using Rhinox.GUIUtils.Editor;
using UnityEditor;
#endif
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    [Serializable]
    public class UnitySafeTypeGenerationSettings
    {
        public List<SerializableType> AdditionalTypes;

        [AssignableTypeFilter, SerializeReference]
        public List<BaseUnitySafeTypeFactory> TypeFactories;

        public void Reinitialize()
        {
            if (TypeFactories == null)
            {
                TypeFactories = new List<BaseUnitySafeTypeFactory>();
                
                PopulateFromCode();
            }
        }

        public void PopulateFromCode()
        {
            foreach (var type in AppDomain.CurrentDomain.GetDefinedTypesOfType<BaseUnitySafeTypeFactory>())
            {
                if (TypeFactories.Any(x => x.GetType() == type))
                    continue;
                
                var factory = Activator.CreateInstance(type) as BaseUnitySafeTypeFactory;
                if (factory == null)
                    continue;
                TypeFactories.Add(factory);
            }
        }
    }
}