using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.Utilities.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Magnus
{
    [ServiceLoader(-4001), ExecutionOrder(-4001)]
    public class PlayerManager : AutoService<PlayerManager>
    {
        protected Player _loadedPlayer;

        public Player ActivePlayer => _loadedPlayer;
        
        public Vector3 ActivePlayerPosition
        {
            get
            {
                if (_loadedPlayer == null)
                    return Vector3.zero;
                return _loadedPlayer.GetPosition();
            }
        }
        
        public delegate void PlayerEventHandler(Player player);

        public event PlayerEventHandler LocalPlayerChanged;
        public event PlayerEventHandler PlayerSpawned;
        public event PlayerEventHandler PlayerKilled;

        
        public void RespawnPlayer(PlayerStart playerStart = null)
        {
            if (playerStart == null)
                RespawnPlayer(Vector3.zero);
            else
                RespawnPlayer(playerStart.transform.position, playerStart.transform.rotation);
        }
        
        public bool RespawnPlayer(Vector3 position, bool persistent = false) => RespawnPlayer(position, Quaternion.identity, persistent);

        public bool RespawnPlayer(Vector3 position, Quaternion rotation, bool persistent = false)
        {
            bool playerIsValid = false;
            if (_loadedPlayer == null)
            {
                PLog.Info<MagnusLogger>($"Spawning new player at pos({position.Print()}) and rot({rotation.eulerAngles.Print()})");
                playerIsValid = SpawnPlayer(position, rotation);
            }
            else
            {
                if (!IsPlayerCompatibleWithSceneConfig(_loadedPlayer))
                {
                    KillPlayer(_loadedPlayer);
                    return RespawnPlayer(position, rotation, persistent);
                }
                PLog.Info<MagnusLogger>($"Moving player to pos({position.Print()}) and rot({rotation.Print()})");
    
                // rotate playArea so the player matches the position and rotation
                _loadedPlayer.SetPositionAndRotation(position, rotation);
                playerIsValid = true;
            }

            if (persistent && !_loadedPlayer.IsPlayerPersistent())
            {
                PLog.Trace<MagnusLogger>($"Making player {_loadedPlayer.name} persistent");
                Object.DontDestroyOnLoad(_loadedPlayer);
            }

            return playerIsValid;
        }
        
        private void SpawnPlayer(Vector3 position, PlayerProfile profile = null) => SpawnPlayer(position, Quaternion.identity, profile);
        
        private bool SpawnPlayer(Vector3 position, Quaternion rotation, PlayerProfile profile = null)
        {
            if (_loadedPlayer != null)
            {
                PLog.Error<MagnusLogger>("Player already spawned");
                return true;
            }
            var config = FindPlayerConfig();
            if (config == null)
            {
                PLog.Error<MagnusLogger>("No PlayerConfig found or configured. Check scene data or Project Settings");
                return false;
            }
            
            _loadedPlayer = config.Load(profile, transform);
            _loadedPlayer.SetPositionAndRotation(position, rotation);
            
            LocalPlayerChanged?.Invoke(_loadedPlayer);
            PlayerSpawned?.Invoke(_loadedPlayer);

            PLog.Trace<MagnusLogger>($"Spawning NEW player {_loadedPlayer.name} at pos({position.Print()}) and rot({rotation.Print()})");
            return true;
        }
        
        public bool KillPlayer(Player player)
        {
            if (_loadedPlayer == null)
                return false;
            
            player.Kill();
            PlayerKilled?.Invoke(player);
            return true;
        }

        public void KillLocalPlayer()
        {
            if (KillPlayer(_loadedPlayer))
                _loadedPlayer = null;
        }
        
        //==============================================================================================================
        // Utility methods

        public bool IsPlayerCompatibleWithSceneConfig(Player player)
        {
            if (player == null)
                return true;
            PlayerConfig config = FindPlayerConfig();
            if (config == MagnusProjectSettings.Instance.PlayerConfig && player.IsPlayerPersistent())
                return true;
            return config == player.LoadedConfig;
        }

        private PlayerConfig FindPlayerConfig()
        {
            var sceneOverrides = Object.FindObjectsOfType<PlayerConfigOverride>();

            sceneOverrides = sceneOverrides.Where(x => LevelLoader.IsSceneActive(x.gameObject.scene)).ToArray();
            
            if (sceneOverrides.Length > 1)
                PLog.Warn<MagnusLogger>($"More than one PlayerConfig override defined in scene, will take the first one to spawn player.");
            var sceneOverride = sceneOverrides.FirstOrDefault(x => x.Config != null);
            PlayerConfig config = sceneOverride != null ? sceneOverride.Config : null;
            if (config == null)
                config = MagnusProjectSettings.Instance.PlayerConfig;
            return config;
        }
    }
}