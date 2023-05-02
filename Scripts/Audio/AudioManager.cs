using System;
using System.Collections.Generic;
using System.Linq;
using ElRaccoone.Tweens;
using ElRaccoone.Tweens.Core;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;


namespace Rhinox.Magnus
{
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioSource GlobalAudioSource;

        private AudioSource _loopingPrefab = null;

        private AudioSource GetSourcePrefab()
        {
            if (_loopingPrefab != null)
                return _loopingPrefab;

            GameObject asGO = new GameObject("[AUTO-GENERATED] Template -- AudioSource");
            asGO.transform.SetParent(transform);
            _loopingPrefab = asGO.AddComponent<AudioSource>();
            _loopingPrefab.GetOrAddComponent<PoolObject>();
            return _loopingPrefab;
        }

        [DictionaryDrawerSettings(KeyLabel = "Trigger", ValueLabel = "Audio")]
        public Dictionary<AudioTrigger, IAudio> AudioBySource = new Dictionary<AudioTrigger, IAudio>();

        private Dictionary<AudioHandle, AudioSource> _usedSourcesByHandle = new Dictionary<AudioHandle, AudioSource>();

        private Dictionary<AudioHandle, Tween<float>>
            _activeTweenByHandle = new Dictionary<AudioHandle, Tween<float>>();

        private List<AudioHandle> _playingHandles = new List<AudioHandle>();

        public delegate void AudioHandleEventHandler(AudioHandle handle);

        public event AudioHandleEventHandler AudioStopped;

        // =================================================================================================================
        // UNITY
        private void Awake()
        {
            if (AudioBySource == null)
                AudioBySource = new Dictionary<AudioTrigger, IAudio>();

            if (_usedSourcesByHandle == null)
                _usedSourcesByHandle = new Dictionary<AudioHandle, AudioSource>();

            foreach (var audioTrigger in AudioBySource.Keys)
            {
                if (!audioTrigger.Enabled)
                    continue;

                audioTrigger.Init();
            }
        }

        private void Update()
        {
            foreach (var handle in _usedSourcesByHandle.Keys)
            {
                var isPlaying = _usedSourcesByHandle[handle].isPlaying;

                if (isPlaying)
                {
                    if (_playingHandles.Contains(handle)) continue;

                    _playingHandles.Add(handle);
                }
                else
                {
                    if (!_playingHandles.Contains(handle)) continue;

                    // Handle could be looping or some other thing; so don't remove it from dict
                    AudioStopped?.Invoke(handle);
                    _playingHandles.Remove(handle);
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var audioTrigger in AudioBySource.Keys)
                audioTrigger.Terminate();

            foreach (var trigger in _usedSourcesByHandle.Keys.ToArray())
                StopAudio(trigger);
            _usedSourcesByHandle.Clear();
            _playingHandles.Clear();

            if (_loopingPrefab != null)
                Utility.Destroy(_loopingPrefab.gameObject);
            base.OnDestroy();
        }

        // =================================================================================================================
        // API - OneShot
        public void PlayOneShot(AudioTrigger audioTrigger)
        {
            if (!CheckClipForTrigger(audioTrigger))
                return;

            IAudio audio = AudioBySource[audioTrigger];
            GlobalAudioSource.PlayOneShot(audio.GetClip(), audio.GetVolume());
        }

        public void PlayOneShot(AudioTrigger audioTrigger, Vector3 location)
        {
            if (!CheckClipForTrigger(audioTrigger))
                return;

            IAudio audio = AudioBySource[audioTrigger];
            AudioSource.PlayClipAtPoint(audio.GetClip(), location, audio.GetVolume());
        }

        public void PlayOneShot(AudioTrigger audioTrigger, Transform host)
        {
            if (!CheckClipForTrigger(audioTrigger))
                return;

            PlayOneShot(AudioBySource[audioTrigger], host);
        }

        public void PlayOneShot(IAudio audio, Transform host)
        {
            PlayClipAtTransform(audio.GetClip(), host, audio.GetVolume());
        }

        public void PlayOneShot(AudioClip audioClip, float volume = 1.0f)
        {
            GlobalAudioSource.PlayOneShot(audioClip, volume);
        }

        // =================================================================================================================
        // API - Handled
        public AudioHandle Play(AudioTrigger audioTrigger, bool looping = false)
        {
            if (!CheckClipForTrigger(audioTrigger))
                return null;

            IAudio audio = AudioBySource[audioTrigger];

            AudioHandle newHandle = new AudioHandle();
            AudioSource audioSrc = FetchNewAudioSource(newHandle, looping);
            audioSrc.clip = audio.GetClip();
            audioSrc.volume = audio.GetVolume();

            audioSrc.Play();
            return newHandle;
        }

        public AudioHandle Play(AudioTrigger audioTrigger, Vector3 location, bool looping = false)
        {
            if (!CheckClipForTrigger(audioTrigger))
                return null;

            IAudio audio = AudioBySource[audioTrigger];

            AudioHandle newHandle = new AudioHandle();
            AudioSource audioSrc = FetchNewAudioSource(newHandle, looping, true);
            audioSrc.clip = audio.GetClip();
            audioSrc.volume = audio.GetVolume();
            audioSrc.transform.position = location;

            audioSrc.Play();
            return newHandle;
        }

        public AudioHandle Play(AudioClip audioClip, bool looping = false, float volume = 1.0f)
        {
            if (audioClip == null)
                return null;

            AudioHandle newHandle = new AudioHandle();
            AudioSource audioSrc = FetchNewAudioSource(newHandle, looping);
            audioSrc.clip = audioClip;
            audioSrc.volume = volume;

            audioSrc.Play();
            return newHandle;
        }

        public AudioHandle Play(AudioClip audioClip, Vector3 location, bool looping = false, float volume = 1.0f)
        {
            if (audioClip == null)
                return null;

            AudioHandle newHandle = new AudioHandle();
            AudioSource audioSrc = FetchNewAudioSource(newHandle, looping, true);
            audioSrc.clip = audioClip;
            audioSrc.volume = volume;
            audioSrc.transform.position = location;

            audioSrc.Play();
            return newHandle;
        }

        public bool IsPlaying() => _playingHandles.Any();

        public bool IsPlaying(AudioHandle handle)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                return false;

            var audioSource = _usedSourcesByHandle[handle];
            return audioSource.isPlaying;
        }

        public bool Reset(AudioHandle handle)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                return false;

            var audioSource = _usedSourcesByHandle[handle];
            audioSource.Stop();
            audioSource.Play();
            return true;
        }

        public void Update(AudioHandle handle, Vector3 location, float volume)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                return;

            var audioSource = _usedSourcesByHandle[handle];
            audioSource.transform.position = location;
            audioSource.volume = volume;
        }

        public void UpdateLocation(AudioHandle handle, Vector3 location)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                return;

            var audioSource = _usedSourcesByHandle[handle];
            audioSource.transform.position = location;
        }

        public void SetVolume(AudioHandle handle, float volume)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                return;

            var audioSource = _usedSourcesByHandle[handle];
            audioSource.volume = volume;
        }

        public void SetRange(AudioHandle handle, float range)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                return;

            var audioSource = _usedSourcesByHandle[handle];
            audioSource.maxDistance = range;
        }

        public void SetNormalizedTime(AudioHandle handle, float normalizedTime)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                return;

            normalizedTime = Mathf.Clamp01(normalizedTime);
            var audioSource = _usedSourcesByHandle[handle];
            audioSource.time = audioSource.clip.length * normalizedTime;
        }

        public void StopAudio(AudioHandle handle)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                return;

            CleanupHandle(handle);
        }

        public void StopAudio(AudioHandle handle, float fadeTime)
        {
            if (fadeTime < float.Epsilon)
            {
                StopAudio(handle);
                return;
            }

            if (!_usedSourcesByHandle.ContainsKey(handle))
                return;

            CleanupTween(handle);

            var source = _usedSourcesByHandle[handle];
            _usedSourcesByHandle.Remove(handle);


            Tween<float> tween = source.TweenAudioSourceVolume(0, fadeTime).SetEaseCubicOut();
            _activeTweenByHandle[handle] = tween;
            tween.SetOnComplete(() =>
            {
                CleanupSource(source);
                CleanupTween(handle);
            });

        }

        private void CleanupHandle(AudioHandle handle)
        {
            var source = _usedSourcesByHandle[handle];
            CleanupSource(source);
            _usedSourcesByHandle.Remove(handle);
            _playingHandles.Remove(handle);

            CleanupTween(handle);
        }

        private void CleanupSource(AudioSource source)
        {
            source.Stop();
            if (source.gameObject)
                ObjectPool.Instance.PushToPool(source.gameObject);
        }

        private void CleanupTween(AudioHandle handle)
        {
            var tween = _activeTweenByHandle.GetOrDefault(handle);
            if (tween == null) return;

            tween.Cancel();

            _activeTweenByHandle.Remove(handle);
        }

        // =================================================================================================================
        // PRIVATE
        private static void PlayClipAtTransform(AudioClip clip, Transform parent, float volume = 1f)
        {
            GameObject gameObject = new GameObject("One shot audio");
            gameObject.transform.SetParent(parent, false);
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.Play();

            Destroy(gameObject, clip.length * Mathf.Max(Time.timeScale, 0.01f));
        }

        private AudioSource FetchNewAudioSource(AudioHandle handle, bool looping, bool spatial = false)
        {
            if (!_usedSourcesByHandle.ContainsKey(handle))
                _usedSourcesByHandle.Add(handle,
                    ObjectPool.Instance.PopFromPool(GetSourcePrefab().gameObject, parent: Instance.transform)
                        .GetComponent<AudioSource>());

            AudioSource source = _usedSourcesByHandle[handle];
            source.loop = looping;
            source.spatialize = spatial;
            source.spatialBlend = spatial ? 1.0f : 0.0f;
            if (source.isPlaying) // Clean object
                source.Stop();
            return source;
        }

        private bool CheckClipForTrigger(AudioTrigger audioTrigger)
        {
            if (!AudioBySource.ContainsKey(audioTrigger))
            {
                PLog.Warn<MagnusLogger>($"No AudioClip configured for AudioSource ({audioTrigger})");
                return false;
            }

            return true;
        }


        [SerializeField, HideInInspector, FormerlySerializedAs("AudioClipsBySource")]
        private Dictionary<AudioTrigger, Audio> _oldAudioClipsBySource = new Dictionary<AudioTrigger, Audio>();

        protected bool HasOldData => !_oldAudioClipsBySource.IsNullOrEmpty();

        [Button, ShowIf(nameof(HasOldData))]
        protected void RecoverOldData()
        {
            if (HasOldData && AudioBySource.IsNullOrEmpty())
            {
                foreach (var (key, value) in _oldAudioClipsBySource)
                    AudioBySource.Add(key, value);
                _oldAudioClipsBySource.Clear();
            }
        }
    }
}