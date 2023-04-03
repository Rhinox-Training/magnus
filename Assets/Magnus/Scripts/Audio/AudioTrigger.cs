using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    [HideReferenceObjectPicker]
    public abstract class AudioTrigger
    {
        [DisplayAsString, ShowInInspector, HideLabel, PropertyOrder(-1)]
        protected virtual string Name => GetType().Name;
        
        [LabelWidth(62.0f)]
        public bool Enabled = true;

        protected AudioTrigger() { Enabled = true; }

        protected List<AudioHandle> _handles;

        public void Init()
        {
            _handles = new List<AudioHandle>();
            OnInit();
        }

        public void Terminate()
        {
            OnTerminate();
        }

        protected abstract void OnInit();
        protected abstract void OnTerminate();
            
        protected AudioHandle Play(bool looping = false)
        {
            if (!AudioManager.HasInstance)
            {
                PLog.Info<MagnusLogger>("No AudioManager found, skipping AudioTrigger...");
                return null;
            }

            var handle = AudioManager.Instance.Play(this, looping);
            _handles.Add(handle);
            return handle;
        }
        
        protected AudioHandle PlayAt(Vector3 location, bool looping = false)
        {
            if (!AudioManager.HasInstance)
            {
                PLog.Info<MagnusLogger>("No AudioManager found, skipping AudioTrigger...");
                return null;
            }
            
            var handle = AudioManager.Instance.Play(this, location, looping);
            _handles.Add(handle);
            return handle;
        }
        
        protected void StopPlaying()
        {
            if (_handles.IsNullOrEmpty())
                return;
            
            if (!AudioManager.HasInstance)
            {
                PLog.Info<MagnusLogger>("No AudioManager found, aborting StopPlaying...");
                return;
            }

            foreach (var handle in _handles)
                AudioManager.Instance.StopAudio(handle);
            _handles.Clear();
        }
    }

    public class AutoPlayAudioTrigger : AudioTrigger
    {
        [LabelWidth(62.0f)]
        public bool Looping = false;

        protected override string Name => "AutoPlay";

        protected override void OnInit()
        {
            Play(Looping);
        }

        protected override void OnTerminate() { }
    }


    
}