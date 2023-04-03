using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class GameModeConfig : ScriptableObject
    {
        public List<GameMode> Modes = new List<GameMode>();

        [ValueDropdown(nameof(GetModeIndices)), LabelText("Default Mode")]
        public int DefaultModeIndex = -1;

        private ICollection<ValueDropdownItem> GetModeIndices()
        {
            if (Modes == null || Modes.Count == 0)
                return Array.Empty<ValueDropdownItem>();
            
            List<ValueDropdownItem> dropdownItems = new List<ValueDropdownItem>()
            {
                new ValueDropdownItem("Null", -1)
            };
            for (int i = 0; i < Modes.Count; ++i)
            {
                if (Modes[i] == null)
                    continue;
                dropdownItems.Add(new ValueDropdownItem(Modes[i].Name ?? "Unnamed GameMode {i}", i));
            }

            return dropdownItems;
        }

        public GameMode GetDefaultMode()
        {
            if (DefaultModeIndex == -1 || Modes == null || Modes.Count == 0)
                return null;

            if (DefaultModeIndex >= 0 && DefaultModeIndex < Modes.Count)
                return Modes[DefaultModeIndex];
            return null;
        }
    }
}