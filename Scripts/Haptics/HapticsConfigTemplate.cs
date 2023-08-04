using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    [HideReferenceObjectPicker, Serializable]
    public class HapticsTemplateEntry
    {
        [HorizontalGroup("Entry"), HideLabel]
        [ValueDropdown(nameof(GetAllSourceTypes))]
        public SerializableType SourceType;

        [HorizontalGroup("Entry"), HideLabel]
        public HapticStrength Strength;

        public HapticsTemplateEntry()
        {
            SourceType = null;
            Strength = HapticStrength.None;
        }

// #if UNITY_EDITOR // TODO: should be editor only, but compile check
        private IEnumerable<ValueDropdownItem> GetAllSourceTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var q = assemblies.SelectMany(assembly => assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => typeof(HapticsSource).IsAssignableFrom(x))
                .Select(x => new ValueDropdownItem(x.Name, new SerializableType(x))));

            return q;
        }
//#endif
    }
    
    public class HapticsConfigTemplate : ScriptableObject
    {
        [SerializeField, ShowInInspector, LabelText("Haptics Entries"),
         ListDrawerSettings(HideAddButton = true, Expanded = true, DraggableItems = false)]
        private List<HapticsTemplateEntry> _hapticsEntries = new List<HapticsTemplateEntry>();

        [HideInInspector] 
        public IReadOnlyCollection<HapticsTemplateEntry> Entries => _hapticsEntries;

#if UNITY_EDITOR
        [Button(ButtonSizes.Medium)]
        private void AddEntry()
        {
            if (_hapticsEntries == null)
                _hapticsEntries = new List<HapticsTemplateEntry>();

            _hapticsEntries.Add(new HapticsTemplateEntry());
        }
#endif
    }
}