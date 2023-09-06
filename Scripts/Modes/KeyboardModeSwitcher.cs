using System;
using System.Collections.Generic;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    [Serializable]
    public class EasyGameModeBind
    {
        public KeyCode KeyCode;
        public GameMode Mode;
    }
    
    public class KeyboardModeSwitcher : MonoBehaviour
    {
        public List<EasyGameModeBind> ModeBinds = new List<EasyGameModeBind>();
        private float _lastPressTime;
        [Range(0.1f, 3.0f), Tooltip("In seconds")]
        public float SpamPreventionWindow = 0.5f;

        public Audio SwitchingAudio;

        private void Awake()
        {
            if (ModeBinds == null)
                ModeBinds = new List<EasyGameModeBind>();

            foreach (var modebind in ModeBinds)
                GameModeManager.Instance.Register(modebind.Mode);
        }
        
        private void Update()
        {
            

            foreach (var modebind in ModeBinds)
            {
                if (Input.GetKeyDown(modebind.KeyCode))
                {
                    if ((Time.realtimeSinceStartup - _lastPressTime) <= SpamPreventionWindow)
                    {
                        PLog.Warn<MagnusLogger>("Prevented spam of GameMode Switching");
                        return;
                    }
                    
                    PLog.TraceDetailed<MagnusLogger>($"Received keyboard bind ModeSwitch");
                    GameModeManager.Instance.SwitchTo(modebind.Mode.Name);
                    if (AudioManager.HasInstance && SwitchingAudio != null && SwitchingAudio.Clip != null)
                        AudioManager.Instance.PlayOneShot(SwitchingAudio.Clip, SwitchingAudio.Volume);
                    _lastPressTime = Time.realtimeSinceStartup;
                    break;
                }
            }
        }
    }
}