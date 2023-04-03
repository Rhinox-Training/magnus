using System;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus
{
    [DisplayAsString]
    public abstract class PlayerModifier
    {
        protected Player _player;

        public virtual void Initialize(Player player)
        {
            _player = player;
        }

        public virtual void Terminate()
        {
            
        }

        public virtual void Update()
        {
            
        }
    }
}