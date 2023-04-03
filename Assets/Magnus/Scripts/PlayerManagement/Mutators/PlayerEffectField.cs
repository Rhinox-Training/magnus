using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    [RequireComponent(typeof(Collider))]
    public class PlayerEffectField : TriggerTracker<Player>
    {
        [ShowInInspector, ValueDropdown(nameof(GetEffectTypes))]
        public SerializableType EffectType;
        
        
        private Dictionary<Player, PlayerModifier> _activeModifiers = new Dictionary<Player,PlayerModifier>();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (EffectType == null)
            {
                var defaultEntry = AppDomain.CurrentDomain.GetDefinedTypesOfType<PlayerModifier>().FirstOrDefault();
                if (defaultEntry != null)
                    EffectType = new SerializableType(defaultEntry);
            }
        }

        protected override void OnDisable()
        {
            foreach (var playerAffected in _activeModifiers.Keys.ToArray())
            {   
                if (playerAffected == null)
                    continue;             
                RemovePlayerEffect(playerAffected);
            }
            base.OnDisable();
        }
        
        private void OnDestroy()
        {
            foreach (var playerAffected in _activeModifiers.Keys.ToArray())
            {   
                if (playerAffected == null)
                    continue;             
                RemovePlayerEffect(playerAffected);
            }
        }

        protected override void OnObjectEnter(Player obj)
        {
            base.OnObjectEnter(obj);
            AddPlayerEffect(obj);
        }

        protected override void OnObjectExit(Player obj)
        {
            RemovePlayerEffect(obj);
            base.OnObjectExit(obj);
        }

        protected override Player GetContainer(Collider coll)
        {
            return coll.gameObject.GetComponentInParent<Player>();
        }

        private void AddPlayerEffect(Player player)
        {
            if (_activeModifiers == null)
                _activeModifiers = new Dictionary<Player, PlayerModifier>();

            if (_activeModifiers.ContainsKey(player))
                return;

            if (EffectType == null)
            {
                PLog.Warn<MagnusLogger>($"No EffectType configured, skipping application to player {player.name}");
                return;
            }
            
            var modifier = Activator.CreateInstance(EffectType) as PlayerModifier;
            if (!player.AddModifier(modifier))
            {
                PLog.Warn<MagnusLogger>($"Effect {modifier} could not be added to player {player.name}");
                return;
            }
            _activeModifiers.Add(player, modifier);
        }

        private ICollection<ValueDropdownItem> GetEffectTypes()
        {
            return AppDomain.CurrentDomain.GetDefinedTypesOfType<PlayerModifier>()
                .Select(x => new ValueDropdownItem(x.Name, new SerializableType(x)))
                .ToArray();
        }

        private void RemovePlayerEffect(Player player)
        {
            if (_activeModifiers == null)
                _activeModifiers = new Dictionary<Player, PlayerModifier>();

            if (!_activeModifiers.ContainsKey(player))
                return;

            var activeModifier = _activeModifiers[player];
            if (activeModifier != null)
                player.RemoveModifier(activeModifier);
            _activeModifiers.Remove(player);
        }
    }
}