using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    [Serializable]
    public class ServiceSettingsEntry
    {
        private const int FIRST_COL_WIDTH = 100;
        private const int LAST_COL_WIDTH = 50;
        
        [HorizontalGroup("TitleRow", FIRST_COL_WIDTH), HideLabel]
        public bool Toggled = true;
        
#if ODIN_INSPECTOR
        [CustomValueDrawer(nameof(OnBaseTypeDrawOdin))]
#endif
        [HorizontalGroup("TitleRow"), SerializeField, HideLabel, DisplayAsString, EnableIf(nameof(Toggled))]
        private SerializableType _baseType;

        [HorizontalGroup("TitleRow", LAST_COL_WIDTH), SerializeField, HideLabel, DisplayAsStringAligned(TextAlignment.Right)]
        private int _servicePriority;

        public int Priority => _servicePriority;

        public Type BaseType => _baseType != null ? _baseType.Type : null;
        
#if ODIN_INSPECTOR
        [TypeFilter(nameof(GetApplicableServiceOptions))]
#else
        [ValueDropdown(nameof(GetApplicableServiceOptions))]
#endif
        [SerializeField, LabelWidth(FIRST_COL_WIDTH), EnableIf(nameof(Toggled)), DoNotDrawAsReference, OnValueChanged(nameof(OnServiceTypeChanged))]
        public SerializableType ServiceType;
        
        public ServiceSettingsEntry(SerializableType serializableType)
        {
            _baseType = serializableType;

            if (_baseType != null && !_baseType.Type.IsAbstract)
            {
                ServiceType = _baseType;
                OnServiceTypeChanged();
            }
        }
        
#if ODIN_INSPECTOR
        private IEnumerable<SerializableType> GetApplicableServiceOptions()
        {
            foreach (var type in Services.GetAvailableServices())
            {
                if (type.InheritsFrom(_baseType))
                    yield return new SerializableType(type);
            }
        }
        
        private SerializableType OnBaseTypeDrawOdin(SerializableType entry, GUIContent content)
        {
            GUILayout.Label(entry.ToString());
            return entry;

        }
#else
        private IEnumerable<ValueDropdownItem<SerializableType>> GetApplicableServiceOptions()
        {
            foreach (var type in Services.GetAvailableServices())
            {
                if (type.InheritsFrom(_baseType))
                    yield return new ValueDropdownItem<SerializableType>(type.GetNameWithNesting(), new SerializableType(type));
            }
        }
#endif
        
        public bool Matches(Type t)
        {
            if (t == null)
                return false;
            return ServiceType == t;
        }

        public void UpdateBase(Type commonBase)
        {
            if (commonBase == null)
            {
                PLog.Error<MagnusLogger>($"commonBase was null, cannot update base for {_baseType.Type.FullName}");
                return;
            }

            if (commonBase == BaseType)
                return;
            
            _baseType = new SerializableType(commonBase);
        }

        private void OnServiceTypeChanged()
        {
            _servicePriority = ServiceType.Type.GetCustomAttribute<ServiceLoaderAttribute>().LoadOrder;
        }
    }
    
    [Serializable]
    public class ServiceSettings
    {
        [ListDrawerSettings(ShowPaging = true, NumberOfItemsPerPage = 12, HideAddButton = true, DraggableItems = false)]
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
                    if (CheckIfHasCommonBase(serviceSetting, type, out Type commonBase))
                    {
                        serviceSetting.UpdateBase(commonBase);
                        hasMatchingService = true;
                        break;
                    }
                }
                if (!hasMatchingService)
                    Services.Add(new ServiceSettingsEntry(serializableType)); // Introduce new base
            }
            
            Services.SortBy(x => x.Priority);
        }

        public bool CheckIfHasCommonBase(ServiceSettingsEntry entry, Type typeCheck, out Type commonBase)
        {
            if (entry.BaseType == null)
            {
                PLog.Error<MagnusLogger>($"Entry {entry.ToString()} was invalid");
                commonBase = null;
                return false;
            }

            if (typeCheck == null || !typeCheck.InheritsFrom(entry.BaseType))
            {
                commonBase = null;
                return false;
            }

            var currentBase = entry.BaseType;
            var possibleInterfaces = typeCheck.GetInterfaces().Where(x => x != typeof(IService) && x.InheritsFrom<IService>());
            var commonInterface = currentBase.GetInterfaces().FirstOrDefault(x => possibleInterfaces.Contains(x));
            if (commonInterface != null)
            {
                commonBase = commonInterface;
                return true;
            }

            Type baseParent = currentBase;
            while (currentBase != null)
            {
                if (!typeCheck.InheritsFrom(currentBase))
                    break;

                if (currentBase.BaseType != null && currentBase.BaseType.IsConstructedGenericType &&
                    currentBase.BaseType.GetGenericTypeDefinition() == typeof(AutoService<>))
                    break;

                baseParent = currentBase;
                currentBase = currentBase.BaseType;
            }

            commonBase = baseParent;
            return true;
        }
        
        /// <summary>
        /// Load service if valid service type and either does not appear in ServiceSettings
        /// or appears in settings and is enabled
        /// </summary>
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