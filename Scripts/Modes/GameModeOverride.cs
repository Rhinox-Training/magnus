using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class GameModeOverride : MonoBehaviour
    {
        [DisableInPlayMode, SerializeReference]
        public GameMode Override;

        private string _oldActiveModeName;

        private void Start()
        {
            if (Override == null)
                return;
            
            GameModeManager.SwitchedMode += OnSwitchedMode;
            _oldActiveModeName = GameModeManager.Instance.ActiveGameModeName;

            if (!GameModeManager.Instance.Register(Override))
                PLog.Warn<MagnusLogger>($"GameModeOverride {Override.Name} already registered.");
            GameModeManager.Instance.SwitchTo(Override.Name);
        }

        private void OnDestroy()
        {
            if (Override == null)
                return;
            GameModeManager.SwitchedMode -= OnSwitchedMode;

            if (!string.IsNullOrWhiteSpace(_oldActiveModeName))
                GameModeManager.Instance.SwitchTo(_oldActiveModeName);
        }
        
        private void OnSwitchedMode(GameModeManager manager, string modename)
        {
            _oldActiveModeName = null;
        }
    }
}