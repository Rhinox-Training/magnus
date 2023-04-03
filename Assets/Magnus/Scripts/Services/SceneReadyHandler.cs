using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus
{
    [ServiceLoader(-15000)]
    public class SceneReadyHandler : AutoService<SceneReadyHandler>
    {
        private static readonly List<Behaviour> _behavioursToToggle = new List<Behaviour>();
        private static readonly Dictionary<Behaviour, bool> _behavioursInitialState = new Dictionary<Behaviour, bool>();

        private bool _sceneIsReady;
        private bool _disabled;
        public bool IsEnabled => _sceneIsReady && !_disabled;

        public delegate void SceneReadyChangeDelegate(bool state);
        public delegate void SceneReadyDelegate();
        
        public event SceneReadyChangeDelegate SceneReadyChange;
        public event SceneReadyDelegate SceneReady;

        protected override void Start()
        {
            _sceneIsReady = true;
            if (!_disabled)
                Enable();
            else
                PLog.Info<MagnusLogger>("SceneReadyHandler passed start but received a disable command. Not activating just yet.");
            
        }

        public static void OnChange(SceneReadyChangeDelegate action, bool executeImmediately = false)
        {
            if (Instance == null) // Should not happen, only from other AutoServices's Awake
            {
                PLog.Error<MagnusLogger>("Tried to subscribe to SceneReadyHandler too early.");
                return;
            }
            
            if (executeImmediately) action?.Invoke(Instance.IsEnabled);
            Instance.SceneReadyChange += action;
        }
        
        public static void OnSceneReady(SceneReadyDelegate action, bool executeIfReady = true)
        {
            if (Instance == null) // Should not happen, only from other AutoServices's Awake
            {
                PLog.Error<MagnusLogger>("Tried to subscribe to SceneReadyHandler too early.");
                return;
            }
            
            if (executeIfReady && Instance.IsEnabled) action?.Invoke();
            Instance.SceneReady += action;
        }
        
        public static void RemoveHandler(SceneReadyDelegate action)
        {
            if (Instance == null) return;
            
            Instance.SceneReady -= action;
        }
        
        public static void RemoveHandler(SceneReadyChangeDelegate action)
        {
            if (Instance == null) return;
            
            Instance.SceneReadyChange -= action;
        }

        /// <summary>
        /// The AddBehaviourToToggleOnLoadedSetupChange method adds a behaviour to the list of behaviours to toggle when `loadedSetup` changes.
        /// </summary>
        /// <param name="behaviour">The behaviour to add.</param>
        public static void YieldToggleControl(Behaviour behaviour, int priority = int.MaxValue)
        {
            if (!_behavioursToToggle.Contains(behaviour))
            {
                _behavioursToToggle.Insert(Mathf.Clamp(priority, 0, _behavioursToToggle.Count), behaviour);
                _behavioursInitialState.Add(behaviour, behaviour.enabled);
            }

            if (!Instance.IsEnabled && behaviour.enabled)
                behaviour.enabled = false;
        }
        
        /// <summary>
        /// The RemoveBehaviourToToggleOnLoadedSetupChange method removes a behaviour of the list of behaviours to toggle when `loadedSetup` changes.
        /// </summary>
        /// <param name="behaviour">The behaviour to remove.</param>
        public static void RevertToggleControl(Behaviour behaviour)
        {
            _behavioursToToggle.Remove(behaviour);
            _behavioursInitialState.Remove(behaviour); // TODO: Restore initial state on gameobject?
        }
        
        public void Enable()
        {
            _disabled = false;

            if (!_sceneIsReady)
            {
                PLog.Info<MagnusLogger>("[SceneReadyHandler::Enable] Canceled due to scene not being ready yet");
                return;
            }
            
            PLog.Info<MagnusLogger>("[SceneReadyHandler::Enable]");
            ToggleBehaviours(true);
            
            SceneReadyChange?.Invoke(true);
            SceneReady?.Invoke();
        }

        public void Disable()
        {
            _disabled = true;
            PLog.Info<MagnusLogger>("[SceneReadyHandler::Disable]");

            if (!_sceneIsReady) // We marked it as disabled but nothing was done yet so just return
                return;
                
            ToggleBehaviours(false);
            
            SceneReadyChange?.Invoke(false);
        }
        
        private void ToggleBehaviours(bool state)
        {
            List<Behaviour> listCopy = _behavioursToToggle.ToList();

            if (!state)
                listCopy.Reverse();

            for (int index = 0; index < listCopy.Count; index++)
            {
                Behaviour behaviour = listCopy[index];
                if (behaviour == null)
                {
                    int loadingIndex = state ? index : _behavioursToToggle.Count - 1 - index;
                    PLog.Error<MagnusLogger>($"A behaviour at index {loadingIndex} to toggle has been destroyed. Have you forgot the corresponding call `SceneReadyHandlerService.RevertToggleControl(this)` in the `OnDestroy` method of `{behaviour.GetType()}`?");
                    _behavioursToToggle.RemoveAt(loadingIndex);

                    continue;
                }
                
                if (!state)
                    PLog.TraceDetailed<MagnusLogger>($"Behaviour ({behaviour}) at index {index} was Disabled.");

                behaviour.enabled = (state && _behavioursInitialState.ContainsKey(behaviour) ? _behavioursInitialState[behaviour] : state);
            }
        }
    }
}