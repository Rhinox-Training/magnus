using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class EmptyConfig : PlayerConfig
    {
        protected override Player CreatePlayer(Transform parent, PlayerProfile profile = null)
        {
            var camera = Camera.main;

            if (camera == null)
                camera = Object.FindObjectsOfType<Camera>().FirstOrDefault(x => x.CompareTag("MainCamera"));
            
            if (camera == null)
            {
                var go = Utility.Create("[GENERATED] Player", parent);
                go.tag = "MainCamera";
                camera = go.AddComponent<Camera>();
            }
            var player = camera.gameObject.GetOrAddComponent<Player>();
            player.Initialize();
            player.Profile = profile;
            
            return player;
        }
    }
}