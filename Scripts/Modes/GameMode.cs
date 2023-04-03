using System.Collections.Generic;
using System.Linq;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class GameMode : ScriptableObject
    {
        public string Name;

        [SerializeReference]
        public List<GameModeModifier> Modifiers = new List<GameModeModifier>();
        
        public bool IsEnabled { get; private set; }

        public void Initialize()
        {
            IsEnabled = false;
        }

        public void Enable()
        {
            if (IsEnabled)
            {
                PLog.Debug<MagnusLogger>($"GameMode {Name} can't be enabled, already active");
                return;
            }

            IsEnabled = true;
            PLog.Debug<MagnusLogger>($"GameMode {Name} switched to {IsEnabled}");
            
            if (Modifiers != null)
            {
                foreach (var modifier in Modifiers)
                {
                    if (modifier == null)
                        continue;
                    modifier.Enable();
                }
            }
        }
        
        public void Disable()
        {
            if (!IsEnabled)
            {
                PLog.Debug<MagnusLogger>($"GameMode {Name} can't be disabled, already inactive");
                return;
            }

            IsEnabled = false;
            PLog.Debug<MagnusLogger>($"GameMode {Name} switched to {IsEnabled}");
            
            if (Modifiers != null)
            {
                foreach (var modifier in Modifiers)
                {
                    if (modifier == null)
                        continue;
                    modifier.Disable();
                }
            }
        }
    }
}