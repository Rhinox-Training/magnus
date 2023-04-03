using Rhinox.Utilities;
using UnityEngine;

namespace Rhinox.Magnus
{
    public abstract class PlayerConfig : ScriptableObject
    {
        public virtual Player Load(PlayerProfile profile, Transform parent)
        {
            Player p = CreatePlayer(parent, profile);
            if (p == null)
                return null;
            p.LoadedConfig = this;
            return p;
        }

        protected abstract Player CreatePlayer(Transform parent, PlayerProfile profile = null);

        public virtual void OnKillPlayer()
        {
            
        }
    }
}