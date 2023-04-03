using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus
{
    [ServiceLoader]
    public class GameModeManager : AutoService<GameModeManager>
    {
        private Dictionary<string, GameMode> _gameModes;
        #if UNITY_EDITOR
        [ShowReadOnlyInPlayMode]
        private IReadOnlyCollection<GameMode> _registeredModes => _gameModes.Values;
        #endif

        [ShowReadOnly]
        private GameMode _activeGameMode = null;

        public bool IsConfigured => _gameModes != null && _gameModes.Count > 0;

        public bool HasActiveGameMode => _activeGameMode != null;

        private string _defaultGameModeKey;

        public string ActiveGameModeName => _activeGameMode?.Name;

        public delegate void GameModeEventHandler(GameModeManager manager, string modeName);
        public static event GameModeEventHandler SwitchedMode;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            if (_gameModes == null)
                _gameModes = new Dictionary<string, GameMode>();
        }

        protected override void Awake()
        {
            base.Awake();
            RegisterGameModesFromConfig();
            if (_defaultGameModeKey != null)
                SwitchTo(_defaultGameModeKey);
        }
        
        protected override void OnNewSceneLoaded(Scene newScene, LoadSceneMode mode)
        {
            if (!IsConfigured)
                return;

            if (_activeGameMode != null)
            {
                PLog.Trace<MagnusLogger>($"GameModeManager - Refreshing mode: {_activeGameMode.Name}");
                _activeGameMode.Disable();
                _activeGameMode.Enable();
            }
        }

        private void RegisterGameModesFromConfig()
        {
            if (MagnusProjectSettings.Instance.GameModes != null)
            {
                var config = MagnusProjectSettings.Instance.GameModes;
                foreach (var mode in config.Modes)
                {
                    if (mode == null)
                        continue;
                    
                    Register(mode, mode == config.GetDefaultMode());
                }
            }
        }

        public bool SwitchToDefault()
        {
            if (_defaultGameModeKey == null)
            {
                PLog.Error<MagnusLogger>(
                    $"GameModeManager - No default game mode configured (register modes: {_gameModes.Count}");
                return false;
            }

            return SwitchTo(_defaultGameModeKey);
        }

        public bool SwitchTo(string name)
        {
            if (!_gameModes.ContainsKey(name))
            {
                PLog.Error<MagnusLogger>($"GameModeManager - Did not find {name} in registered gameModes, aborting...");
                return false;
            }

            if (_activeGameMode != null)
            {
                if (_activeGameMode.Name.Equals(name, StringComparison.InvariantCulture))
                {
                    PLog.Debug<MagnusLogger>($"GameModeManager - Already in GameMode '{name}', aborting switch...");
                    return false;
                }
                
                _activeGameMode.Disable();
            }

            PLog.Info<MagnusLogger>($"GameModeManager - Switching to mode: {name}");
            _activeGameMode = _gameModes[name];
            _activeGameMode.Enable();
            SwitchedMode?.Invoke(this, name);
            return true;
        }

        public IReadOnlyCollection<string> GetModeNames()
        {
            return _gameModes.Keys.ToArray();
        }

        public bool SetDefaultMode(GameMode mode)
        {
            if (mode == null)
                return false;
            return SetDefaultMode(mode.Name);
        }

        public bool SetDefaultMode(string modeKey)
        {
            if (!_gameModes.ContainsKey(modeKey))
            {
                PLog.Warn<MagnusLogger>($"GameModeManager - No registered GameMode with name: {modeKey}.");
                return false;
            }
            
            PLog.Debug<MagnusLogger>($"GameModeManager - Setting default game mode to '{modeKey}'");
            _defaultGameModeKey = modeKey;
            return true;
        }

        public bool Register(GameMode newMode, bool newDefault = false)
        {
            string modeKey = newMode.Name;
            if (_gameModes.ContainsKey(modeKey))
            {
                PLog.Warn<MagnusLogger>($"GameModeManager - Already registered GameMode with name: {newMode.Name}.");
                return false;
            }

            if (newDefault || _gameModes.Count == 0)
            {
                if (_defaultGameModeKey != null)
                    PLog.Debug<MagnusLogger>($"GameModeManager - Overwriting default game mode '{_defaultGameModeKey}' with '{modeKey}'");
                _defaultGameModeKey = modeKey;
            }

            newMode.Initialize();
            _gameModes.Add(modeKey, newMode);
            return true;
        }

        public bool Deregister(GameMode mode)
        {
            string modeKey = mode.Name;
            if (!_gameModes.ContainsKey(modeKey))
            {
                PLog.Warn<MagnusLogger>($"GameModeManager - No registered GameMode with name: {modeKey}.");
                return false;
            }

            if (modeKey.Equals(_defaultGameModeKey)) // Remove default if gameMode it references no longer available
            {
                if (_gameModes.Count == 0)
                    _defaultGameModeKey = null;
                else
                    _defaultGameModeKey = _gameModes.First().Key; // TODO: better way to select new default?
            }

            _gameModes.Remove(modeKey);
            return true;
        }

        public bool Deregister(string modeName)
        {
            if (!_gameModes.ContainsKey(modeName))
            {
                PLog.Warn<MagnusLogger>($"GameModeManager - No registered GameMode with name: {modeName}.");
                return false;
            }

            _gameModes.Remove(modeName);
            return true;
        }
    }
}
