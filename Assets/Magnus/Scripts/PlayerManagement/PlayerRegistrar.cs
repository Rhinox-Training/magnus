using Rhinox.Perceptor;
using UnityEngine;

namespace Rhinox.Magnus
{
    [RequireComponent(typeof(Player))]
    public class PlayerRegistrar : MonoBehaviour
    {
        private Player _player;

        protected virtual void Awake()
        {
            _player = GetComponent<Player>();
            PlayerManager.Instance.RegisterPlayer(_player.Profile, _player); // TODO: is this a safe way to handle profile?
        }
    }
}