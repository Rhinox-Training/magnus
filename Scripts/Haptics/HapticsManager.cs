using System;
using System.Collections.Generic;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus
{
    public enum HapticStrength
    {
        None,
        Small,
        Medium,
        Large
    }

    [ServiceLoader(-1)]
    public class HapticsManager : AutoService<HapticsManager>
    {
        public HapticsConfig Haptics;
        private Dictionary<Type, HapticsDeviceHandler> _configuredHandlers;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            if (Haptics == null)
                Haptics = new HapticsConfig();

            Haptics.Initialize();

            _configuredHandlers = new Dictionary<Type, HapticsDeviceHandler>();
            foreach (var type in AppDomain.CurrentDomain.GetDefinedTypesWithAttribute<HapticsHandlerAttribute>())
            {
                if (!type.InheritsFrom(typeof(HapticsDeviceHandler)))
                    continue;
                
                var instance = Activator.CreateInstance(type) as HapticsDeviceHandler;
                _configuredHandlers.Add(type, instance);
            }
        }

        protected override void OnDestroy()
        {
            Haptics.Terminate();
            
            if (_configuredHandlers != null)
                _configuredHandlers.Clear();
            
            base.OnDestroy();
        }

        public void Register(HapticsConfigTemplate template, bool overrideEntries = false)
        {
            foreach (var entry in template.Entries)
            {
                if (entry.SourceType == null)
                {
                    PLog.Info<MagnusLogger>($"HapticsManager - Null entry in template, skipping registration...");
                    continue;
                }
                
                if (!Haptics.Register(entry, overrideEntries))
                    PLog.Warn<MagnusLogger>($"HapticsManager - Failed to register haptics entry {entry}.");
            }
        }

        public void ClearConfig()
        {
            if (Haptics != null)
                Haptics.Clear();
        }
        
        public void ExecuteHaptics(DeviceHapticSpecifier specifier, HapticsSource source)
        {
            if (Haptics == null || !Haptics.Contains(source))
            {
                PLog.Error<MagnusLogger>($"Cannot execute haptics, source {source} is not registered");
                return;
            }

            var specifierType = specifier?.GetType();
            if (specifier == null || !_configuredHandlers.ContainsKey(specifierType))
            {
                PLog.Error<MagnusLogger>($"Cannot execute haptics, specifier {specifier} has no registered handler");
                return;
            }

            _configuredHandlers[specifierType].HandleHaptics(specifier, Haptics[source]);
        }
    }
}