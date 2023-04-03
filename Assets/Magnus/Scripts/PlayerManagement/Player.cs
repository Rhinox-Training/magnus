using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using UnityEngine;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine.XR;

namespace Rhinox.Magnus
{
    public class Player : MonoBehaviour
    {
        [ShowReadOnlyInPlayMode]
        private List<PlayerModifier> _modifiers;

        public PlayerConfig LoadedConfig { get; internal set; }

        public virtual Vector3 ViewDirection => transform.forward;  // TODO: should make Player abstract?
        
        public virtual Vector3 ViewOrigin => transform.position; // TODO: should make Player abstract?
        public PlayerProfile Profile { get; set; }

        public delegate void PlayerEventHandler(Player player);

        public event PlayerEventHandler Initialized;
        public static event PlayerEventHandler GlobalInitialized;
        
        
        protected virtual void Awake()
        {
            if (_modifiers == null)
                _modifiers = new List<PlayerModifier>();
        }

        protected virtual void OnDestroy()
        {
            PLog.Trace<MagnusLogger>($"[Player] OnDestroy {name} - child of {transform.root.name}");
        }

        public void Initialize()
        {
            OnInitialize();

            Initialized?.Invoke(this);
            GlobalInitialized?.Invoke(this);
        }

        protected virtual void OnInitialize()
        {
            
        }

        [Button]
        public bool AddModifier(PlayerModifier mod)
        {
            if (mod == null || _modifiers.Contains(mod))
                return false;
            PLog.Info<MagnusLogger>($"[Player] Effect '{mod.GetType().Name}' applied to '{name}'");
            _modifiers.Add(mod);
            mod.Initialize(this);
            return true;
        }

        public bool RemoveModifier(PlayerModifier mod)
        {
            if (mod == null || !_modifiers.Contains(mod))
                return false;
            PLog.Info<MagnusLogger>($"[Player] Effect '{mod.GetType().Name}' removed from '{name}'");
            _modifiers.Remove(mod);
            mod.Terminate();
            return true;
        }

        [Button]
        public void ClearAllModifiers()
        {
            foreach (var mod in _modifiers)
                mod.Terminate();
            _modifiers.Clear();
        }

        protected virtual void Update()
        {
            foreach (var mod in _modifiers)
                mod.Update();
        }

        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        public virtual Vector3 GetPosition()
        {
            return transform.position;
        }

        public virtual void Kill()
        {
            if (LoadedConfig != null)
                LoadedConfig.OnKillPlayer();
            gameObject.SetActive(false);
            Utility.Destroy(gameObject);
        }
    }
}