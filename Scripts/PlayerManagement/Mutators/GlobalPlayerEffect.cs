using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class GlobalPlayerEffect : MonoBehaviour
    {
        [AssignableTypeFilter(typeof(PlayerModifier))]
        public SerializableType EffectType;
        
        private Dictionary<Player, PlayerModifier> _activeModifiers = new Dictionary<Player,PlayerModifier>();

        private Player _subscribedPlayer;
        
        protected virtual void OnEnable()
        {
            _activeModifiers = new Dictionary<Player, PlayerModifier>();
            TryAddPlayerEffect();
        }

        private void TryAddPlayerEffect()
        {
            _subscribedPlayer = PlayerManager.Instance.ActivePlayer;
            if (_subscribedPlayer)
                AddPlayerEffect(_subscribedPlayer);
        }

        private void Update()
        {
            if (_subscribedPlayer == null)
                TryAddPlayerEffect();
        }

        protected void OnDisable()
        {
            RemovePlayerEffect(PlayerManager.Instance.ActivePlayer);
        }

        private void Reset()
        {
            var defaultEntry = AppDomain.CurrentDomain.GetDefinedTypesOfType<PlayerModifier>().FirstOrDefault();
            if (defaultEntry != null)
                EffectType = new SerializableType(defaultEntry);
            else
                EffectType = null;
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

        private void RemovePlayerEffect(Player player)
        {
            if (player == null) return;
            
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