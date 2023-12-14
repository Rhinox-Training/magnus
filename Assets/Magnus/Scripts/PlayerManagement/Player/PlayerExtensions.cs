using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus
{
    public static class PlayerExtensions
    {
        public static Ray GetViewRay(this Player p)
        {
            return new Ray(p.ViewOrigin, p.ViewDirection);
        }

        public static bool IsPlayerPersistent(this Player p)
        {
            return p != null && p.gameObject.scene != SceneManager.GetActiveScene();
        }
        
        public static bool TryGetPlayerProfile<T>(this Player p, out T playerProfile) where T : PlayerProfile
        {
            playerProfile = null;
            if (p == null)
                return false;
            
            if (p.Profile is T activePlayerProfile)
            {
                playerProfile = activePlayerProfile;
                return true;
            }
            
            return false;
        }
    }
}