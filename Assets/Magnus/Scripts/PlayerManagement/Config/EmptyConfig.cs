using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using UnityEngine;

namespace Rhinox.Magnus
{
    public class EmptyConfig : PlayerConfig
    {
        public bool CreateCameraIfNotFound = true;
        
        protected override Player CreatePlayer(Transform parent, PlayerProfile profile = null)
        {
            GameObject playerObject;
            var camera = Camera.main;

            if (camera == null) // TODO: this is redundant? Camera.main returns object with MainCamera tag if it exists
                camera = Object.FindObjectsOfType<Camera>().FirstOrDefault(x => x.CompareTag("MainCamera"));
            
            if (camera == null)
            {
                playerObject = Utility.Create("[GENERATED] Player", parent);
                if (CreateCameraIfNotFound)
                {
                    playerObject.tag = "MainCamera";
                    playerObject.AddComponent<Camera>();
                }
            }
            else
                playerObject = camera.gameObject;
            
            var player = playerObject.GetOrAddComponent<Player>();
            player.Initialize();
            player.Profile = profile;
            
            return player;
        }
    }
}