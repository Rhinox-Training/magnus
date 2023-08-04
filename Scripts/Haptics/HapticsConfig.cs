using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Rhinox.Magnus
{
    [HideReferenceObjectPicker]
    public class HapticsConfig
    {
        [ShowReadOnlyInPlayMode]
        private Dictionary<HapticsSource, HapticStrength> _registeredHaptics;
        [ShowReadOnlyInPlayMode]
        private bool _initialized;

        public bool Contains(HapticsSource source)
        {
            if (source == null)
                return false;
            return _registeredHaptics.ContainsKey(source);
        }

        public HapticStrength this[HapticsSource source]
        {
            get { return _registeredHaptics[source]; }
        }
        
        public HapticsConfig()
        {
            _registeredHaptics = new Dictionary<HapticsSource, HapticStrength>();
            _initialized = false;
        }
        
        public void Initialize()
        {
            if (_initialized)
            {
                PLog.Trace<MagnusLogger>("Already initialized this HapticsConfig, skipping Initialize...");
                return;
            }
            
            if (_registeredHaptics == null)
                _registeredHaptics = new Dictionary<HapticsSource, HapticStrength>();


            foreach (var hapticsSource in _registeredHaptics.Keys)
            {
                if (hapticsSource == null)
                    continue;
                
                hapticsSource.Init();
            }
            
            _initialized = true;
        }

        public void Terminate()
        {
            if (!_initialized)
            {
                PLog.Trace<MagnusLogger>("HapticsConfig was not initialized, skipping Terminate...");
                return;
            }
            
            foreach (var hapticsSource in _registeredHaptics.Keys)
            {
                if (hapticsSource == null)
                    continue;
                
                hapticsSource.Terminate();
            }
            
            _initialized = false;
        }

        public bool Register(HapticsTemplateEntry entry, bool overrideEntries = false)
        {
            if (entry == null)
                return false;
            
            if (_registeredHaptics == null)
                _registeredHaptics = new Dictionary<HapticsSource, HapticStrength>();

            HapticsSource oldHapticsSource = _registeredHaptics.Keys.FirstOrDefault(x => x.GetType().Equals(entry.SourceType));
            if (oldHapticsSource != null)
            {
                if (overrideEntries)
                {
                    PLog.Debug<MagnusLogger>($"Removing entry of Type{entry.SourceType}, registering new value...");
                    oldHapticsSource.Terminate();
                    _registeredHaptics.Remove(oldHapticsSource);
                }
                else
                {
                    PLog.TraceDetailed<MagnusLogger>($"Skipping registration of template entry {entry} - Type {entry.SourceType} already registered...");
                    return false;
                }
            }

            var source = (HapticsSource) Activator.CreateInstance(entry.SourceType);
            _registeredHaptics.Add(source, entry.Strength);
            PLog.Info<MagnusLogger>($"Registered haptics for {entry.SourceType.Name} (setting: {entry.Strength.ToString()})");
            
            if (_initialized)
                source.Init();
            
            return true;
        }
        
        public void Clear()
        {
            if (_registeredHaptics == null)
                return;
            
            foreach (var hapticsSource in _registeredHaptics.Keys)
            {
                if (hapticsSource == null)
                    continue;
                
                hapticsSource.Terminate();
            }
            
            _registeredHaptics.Clear();
        }

#if UNITY_EDITOR
        private IEnumerable<Type> GetAllSourceTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var q = assemblies.SelectMany(assembly => assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(HapticsSource).IsAssignableFrom(x)));

            return q;
        }
#endif
    }


}