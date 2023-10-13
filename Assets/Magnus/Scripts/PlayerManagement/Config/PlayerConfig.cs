using System.Collections.Generic;
using Rhinox.Utilities;
using UnityEngine;

namespace Rhinox.Magnus
{
    public abstract class PlayerConfig : ScriptableObject
    {
        public List<IPlayerSubsystem> PlayerSubsystems;
        
        public virtual Player Load(PlayerProfile profile, Transform parent)
        {
            Player p = CreatePlayer(parent, profile);
            if (p == null)
                return null;
            p.LoadedConfig = this;
            if (PlayerSubsystems != null)
            {
                foreach (var subSystem in PlayerSubsystems)
                {
                    if (subSystem == null)
                        continue;
                    subSystem.Initialize(p);
                }
            }
            return p;
        }

        protected abstract Player CreatePlayer(Transform parent, PlayerProfile profile = null);

        public virtual void OnKillPlayer()
        {
            if (PlayerSubsystems != null)
            {
                foreach (var subSystem in PlayerSubsystems)
                {
                    if (subSystem == null)
                        continue;
                    subSystem.Terminate();
                }
            }
        }
    }
}