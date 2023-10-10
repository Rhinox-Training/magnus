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
        protected PlayerProfile _currentPlayerProfile;

        [ShowReadOnlyInPlayMode]
        public Player ActivePlayer => _loadedPlayer;
        public bool IsPlayerPersistent => _loadedPlayer != null && _loadedPlayer.gameObject.scene != SceneManager.GetActiveScene();

        private Transform _playArea;

        public Vector3 ActivePlayerPosition
        {
            get
            {
                if (_loadedPlayer == null)
                    return Vector3.zero;
                return _loadedPlayer.GetPosition();
            }
        }

        public bool LoggedIn => _currentPlayerProfile != null;
        
        public delegate void PlayerEventHandler(Player player);

        public event PlayerEventHandler PlayerChanged; 

        public void Login(PlayerProfile profile) // TODO: multiplayer support
        {
            _currentPlayerProfile = profile;
            if (_loadedPlayer != null)
                _loadedPlayer.Profile = profile;
        }

        public void Logout()
        {
            _currentPlayerProfile = null;
            if (_loadedPlayer != null)
                _loadedPlayer.Profile = null;
        }
        
        public T GetPlayerProfile<T>() where T : PlayerProfile, new()
        {
            var activePlayerProfile = Instance.ActivePlayer.Profile as T;
            return activePlayerProfile ?? new T();
        }

        private PlayerStart FindSpawnLocation(GuidAsset playerStartAsset = null)
        {
            if (playerStartAsset != null)
            {
                var identifier = GuidIdentifier.GetFor(playerStartAsset, typeof(PlayerStart));
                if (identifier != null)
                    return (PlayerStart) identifier.TargetComponent;
                else
                    PLog.Warn<MagnusLogger>("FindSpawnLocation was passed a PlayerStart but it could not be found...");
            }
            
            var spawnLocations = Object.FindObjectsOfType<PlayerStart>();
            if (spawnLocations.Length <= 1)
                return spawnLocations.FirstOrDefault();

            return spawnLocations.GetRandomObject();
        }
        
        public void RespawnPlayer(GuidAsset playerStartAsset = null)
        {
            PlayerStart playerStart = FindSpawnLocation(playerStartAsset);
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
                if (!IsActivePlayerCompatibleWithSceneConfig())
                {
                    KillPlayer();
                    return RespawnPlayer(position, rotation, persistent);
                }
                PLog.Info<MagnusLogger>($"Moving player to pos({position.Print()}) and rot({rotation.Print()})");
    
                // rotate playArea so the player matches the position and rotation
                _loadedPlayer.SetPositionAndRotation(position, rotation);
                playerIsValid = true;
            }

            if (persistent && !IsPlayerPersistent)
            {
                PLog.Trace<MagnusLogger>($"Making player {_loadedPlayer.name} persistent");
                Object.DontDestroyOnLoad(_loadedPlayer);
            }

            return playerIsValid;
        }
        
        private void SpawnPlayer(Vector3 position) => SpawnPlayer(position, Quaternion.identity);
        
        private bool SpawnPlayer(Vector3 position, Quaternion rotation)
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
            
            _loadedPlayer = config.Load(_currentPlayerProfile, transform);
            _loadedPlayer.SetPositionAndRotation(position, rotation);
            
            PlayerChanged?.Invoke(_loadedPlayer);

            PLog.Trace<MagnusLogger>($"Spawning NEW player {_loadedPlayer.name} at pos({position.Print()}) and rot({rotation.Print()})");
            return true;
        }
        
        public void KillPlayer()
        {
            if (_loadedPlayer == null)
                return;
            
            _loadedPlayer.Kill();
            _loadedPlayer = null;
        }

        public bool IsActivePlayerCompatibleWithSceneConfig()
        {
            if (_loadedPlayer == null)
                return true;
            PlayerConfig config = FindPlayerConfig();
            if (config == MagnusProjectSettings.Instance.PlayerConfig && IsPlayerPersistent)
                return true;
            return config == _loadedPlayer.LoadedConfig;
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