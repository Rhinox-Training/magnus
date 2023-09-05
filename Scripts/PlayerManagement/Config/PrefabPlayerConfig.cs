using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class PrefabPlayerConfig : PlayerConfig
    {
        public GameObject Prefab;
        
        protected override Player CreatePlayer(Transform parent, PlayerProfile profile = null)
        {
            var playerObject = Instantiate(Prefab);
            var player = playerObject.GetOrAddComponent<Player>();
            player.Initialize();
            player.Profile = profile;
            
            return player;
        }
    }
}