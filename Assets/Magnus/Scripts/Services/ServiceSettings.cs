using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    [Serializable]
    public class ServiceSettingsEntry
    {
        private const int FIRST_COL_WIDTH = 100;
        
        [HorizontalGroup("TitleRow", FIRST_COL_WIDTH), HideLabel]
        public bool Toggled = true;
        [HorizontalGroup("TitleRow"), SerializeField, DisplayAsString, EnableIf(nameof(Toggled))]
        private SerializableType _baseType;
        
        [ValueDropdown(nameof(OnValueChanged)), LabelWidth(FIRST_COL_WIDTH), EnableIf(nameof(Toggled))]
        public SerializableType ServiceType;
        
        public ServiceSettingsEntry(SerializableType serializableType)
        {
            _baseType = serializableType;
        }
        
        private IEnumerable<ValueDropdownItem> OnValueChanged()
        {
            foreach (var type in Services.GetAvailableServices())
            {
                if (type.InheritsFrom(_baseType))
                    yield return new ValueDropdownItem(type.GetNameWithNesting(), new SerializableType(type));
            }
        }

        public bool Matches(SerializableType serializableType)
        {
            if (serializableType == null)
                return false;
            return _baseType == serializableType || serializableType.Type.InheritsFrom(_baseType.Type);
        }

        public bool Matches(Type t)
        {
            if (t == null)
                return false;
            return _baseType == t || t.InheritsFrom(_baseType.Type);
        }
    }
    
    [Serializable]
    public class ServiceSettings
    {
        [ListDrawerSettings(ShowPaging = true, NumberOfItemsPerPage = 12, HideAddButton = true)]
        public List<ServiceSettingsEntry> Services;

        [Button("Update Service Entries")]
        public void Reinitialize()
        {
            if (Services == null)
                Services = new List<ServiceSettingsEntry>();

            foreach (var type in Rhinox.Magnus.Services.GetAvailableServices())
            {
                var serializableType = new SerializableType(type);
                bool hasMatchingService = false;
                foreach (var serviceSetting in Services)
                {
                    if (serviceSetting == null)
                        continue;
                    if (!serviceSetting.Matches(serializableType))
                        continue;
                    hasMatchingService = true;
                }
                if (!hasMatchingService)
                    Services.Add(new ServiceSettingsEntry(serializableType));
            }
        }
        
        public bool ShouldLoadService(Type serviceType)
        {
            if (serviceType == null || !serviceType.InheritsFrom<IService>())
                return false;

            if (Services == null)
                return true;
            foreach (var serviceSetting in Services)
            {
                if (serviceSetting == null)
                    continue;
                if (serviceSetting.Matches(serviceType))
                    return serviceSetting.Toggled;
            }
            return true;
        }

        public bool ShouldLoadService<T>() where T : IService => ShouldLoadService(typeof(T));
    }
}