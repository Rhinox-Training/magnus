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
        protected List<PlayerProfile> _generatedPlayers;

        public Player ActivePlayer => _activePlayers != null && _activePlayers.ContainsKey(MagnusProjectSettings.Instance.LocalPlayerProfile) ? 
            _activePlayers[MagnusProjectSettings.Instance.LocalPlayerProfile] : 
            null;
        
        public delegate void PlayerEventHandler(Player player);

        public event PlayerEventHandler LocalPlayerChanged;
        public event PlayerEventHandler PlayerSpawned;
        public event PlayerEventHandler PlayerKilled;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _activePlayers = new Dictionary<PlayerProfile, Player>();
            _generatedPlayers = new List<PlayerProfile>();
        }
        
        //==============================================================================================================
        // Local player specific

        public void RespawnLocalPlayer(PlayerStart playerStart = null)
        {
            if (playerStart == null)
                RespawnLocalPlayer(Vector3.zero);
            else
                RespawnLocalPlayer(playerStart.transform.position, playerStart.transform.rotation);
        }

        public bool RespawnLocalPlayer(Vector3 position, Quaternion rotation, bool persistent = false) =>
            RespawnPlayer(MagnusProjectSettings.Instance.LocalPlayerProfile, position, rotation, persistent);
        
        public bool RespawnLocalPlayer(Vector3 position, bool persistent = false) => RespawnPlayer(MagnusProjectSettings.Instance.LocalPlayerProfile, position, Quaternion.identity, persistent);
        
        public void KillLocalPlayer()
        {
            KillPlayer(MagnusProjectSettings.Instance.LocalPlayerProfile);
        }
        
        //==============================================================================================================
        // General API

        public Player GetActivePlayer(PlayerProfile profile)
        {
            if (profile == null)
            {
                PLog.Error<MagnusLogger>($"Null profile not supported in search for player...");
                return null;
            }

            if (_activePlayers == null || !_activePlayers.ContainsKey(profile))
                return null;
            return _activePlayers[profile];
        }

        public int GetActivePlayerCount()
        {
            return _activePlayers != null ? _activePlayers.Count : 0;
        }
        
        public bool RespawnPlayer(PlayerProfile profile, Vector3 position, Quaternion rotation, bool persistent = false)
        {
            bool playerIsValid = false;
            var player = _activePlayers.ContainsKey(profile) ? _activePlayers[profile] : null;
            if (player == null)
            {
                PLog.Info<MagnusLogger>($"Spawning new player at pos({position.Print()}) and rot({rotation.eulerAngles.Print()})");
                playerIsValid = SpawnPlayer(position, rotation, profile, persistent);
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

        public bool RegisterPlayer(PlayerProfile profile, Player player)
        {
            if (profile == null || player == null)
            {
                PLog.Error<MagnusLogger>($"Cannot register a player '{player}' to a profile '{profile}' player if one or both are null...");
                return false;
            }

            if (_activePlayers.ContainsKey(profile))
            {
                if (_generatedPlayers.Contains(profile))
                {
                    if (!KillPlayer(profile))
                    {
                        PLog.Error<MagnusLogger>($"Could not kill generated player {_activePlayers[profile]}...");
                        return false;
                    }
                }
                else
                {
                    PLog.Error<MagnusLogger>($"Could not register new player for profile {profile}, already has a registered non-generated player {_activePlayers[profile]}");
                    return false;
                }
            }

            _activePlayers.Add(profile, player);
            return true;
        }

        public bool DeregisterPlayer(PlayerProfile profile, out Player player)
        {
            if (profile == null)
            {
                PLog.Error<MagnusLogger>($"Cannot deregister a null profile, unsupported...");
                player = null;
                return false;
            }

            if (!_activePlayers.ContainsKey(profile))
            {
                PLog.Warn<MagnusLogger>($"No player found for profile '{profile}'...");
                player = null;
                return false;
            }

            if (_generatedPlayers.Contains(profile))
            {
                PLog.Warn<MagnusLogger>($"Cannot deregister a generated player...");
                player = null;
                return false;
            }

            player = _activePlayers[profile];
            _activePlayers.Remove(profile);
            return true;
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
            _generatedPlayers.Add(profile);
            
            TriggerPlayerSpawnEvents(profile, newPlayer);

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
            if (!player.Kill())
            {
                PLog.Debug<MagnusLogger>($"Cannot kill player '{playerProfile}'");
                return false;
            }
            
            PlayerKilled?.Invoke(player);
            _activePlayers.Remove(playerProfile);
            _generatedPlayers.Remove(playerProfile);
            return true;
        }
        
        //==============================================================================================================
        // Utility methods

        private void TriggerPlayerSpawnEvents(PlayerProfile profile, Player newPlayer)
        {
            if (profile == MagnusProjectSettings.Instance.LocalPlayerProfile)
                LocalPlayerChanged?.Invoke(newPlayer);
            PlayerSpawned?.Invoke(newPlayer);
        }

        public bool IsPlayerCompatibleWithSceneConfig(Player player)
        {
            // Registered non-generated players are always compatible
            if (player == null || !_generatedPlayers.Contains(player.Profile)) 
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