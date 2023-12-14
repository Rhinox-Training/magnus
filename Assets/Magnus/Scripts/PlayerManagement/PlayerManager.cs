using System.Collections.Generic;
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
        protected Dictionary<PlayerProfile, Player> _activePlayers;

        public Player ActivePlayer => _activePlayers != null && _activePlayers.ContainsKey(MagnusProjectSettings.Instance.LocalPlayerProfile) ? _activePlayers[MagnusProjectSettings.Instance.LocalPlayerProfile] : null;
        
        public delegate void PlayerEventHandler(Player player);

        public event PlayerEventHandler LocalPlayerChanged;
        public event PlayerEventHandler PlayerSpawned;
        public event PlayerEventHandler PlayerKilled;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _activePlayers = new Dictionary<PlayerProfile, Player>();
        }

        public void RespawnPlayer(PlayerStart playerStart = null)
        {
            if (playerStart == null)
                RespawnLocalPlayer(Vector3.zero);
            else
                RespawnLocalPlayer(playerStart.transform.position, playerStart.transform.rotation);
        }

        public bool RespawnLocalPlayer(Vector3 position, Quaternion rotation, bool persistent = false) =>
            RespawnPlayer(MagnusProjectSettings.Instance.LocalPlayerProfile, position, rotation, persistent);
        
        public bool RespawnLocalPlayer(Vector3 position, bool persistent = false) => RespawnPlayer(MagnusProjectSettings.Instance.LocalPlayerProfile, position, Quaternion.identity, persistent);
        
        public bool RespawnPlayer(PlayerProfile profile, Vector3 position, Quaternion rotation, bool persistent = false)
        {
            bool playerIsValid = false;
            var player = _activePlayers.ContainsKey(profile) ? _activePlayers[profile] : null;
            if (player == null)
            {
                PLog.Info<MagnusLogger>($"Spawning new player at pos({position.Print()}) and rot({rotation.eulerAngles.Print()})");
                playerIsValid = SpawnPlayer(position, rotation, profile,persistent);
            }
            else if (!IsPlayerCompatibleWithSceneConfig(player))
            {
                KillPlayer(profile);
                playerIsValid = SpawnPlayer(position, rotation, profile, persistent);
            }
            else
            {
                PLog.Info<MagnusLogger>($"Moving player to pos({position.Print()}) and rot({rotation.Print()})");
                // rotate playArea so the player matches the position and rotation
                player.SetPositionAndRotation(position, rotation);
                playerIsValid = true;
            }
            return playerIsValid;
        }
        
        private bool SpawnPlayer(Vector3 position, Quaternion rotation, PlayerProfile profile, bool persistent = false)
        {
            if (profile == null)
            {
                PLog.Error<MagnusLogger>("Cannot spawn player without a profile...");
                return false;
            }
            
            if (_activePlayers.ContainsKey(profile))
            {
                PLog.Error<MagnusLogger>($"Player '{profile}' already spawned");
                return true;
            }
            var config = FindPlayerConfig();
            if (config == null)
            {
                PLog.Error<MagnusLogger>("No PlayerConfig found or configured. Check scene data or Project Settings");
                return false;
            }
            
            var newPlayer = config.Load(profile, transform);
            newPlayer.SetPositionAndRotation(position, rotation);

            if (persistent && !newPlayer.IsPlayerPersistent())
            {
                PLog.Trace<MagnusLogger>($"Making player {newPlayer.name} persistent");
                Object.DontDestroyOnLoad(newPlayer);
            }


            _activePlayers.Add(profile, newPlayer);
            
            LocalPlayerChanged?.Invoke(newPlayer);
            PlayerSpawned?.Invoke(newPlayer);

            PLog.Trace<MagnusLogger>($"Spawning NEW player {newPlayer.name} at pos({position.Print()}) and rot({rotation.Print()})");
            return true;
        }
        
        public bool KillPlayer(PlayerProfile playerProfile)
        {
            if (playerProfile == null)
                return false;

            if (!_activePlayers.ContainsKey(playerProfile))
            {
                PLog.Error<MagnusLogger>($"Player {playerProfile} was not registered, manager has no authority to kill this player...");
                return false;
            }
            
            var player = _activePlayers[playerProfile];
            player.Kill();
            PlayerKilled?.Invoke(player);
            _activePlayers.Remove(playerProfile);
            return true;
        }

        public void KillLocalPlayer()
        {
            KillPlayer(MagnusProjectSettings.Instance.LocalPlayerProfile);
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