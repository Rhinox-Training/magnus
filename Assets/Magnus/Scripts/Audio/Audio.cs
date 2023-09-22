using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    public interface IAudio
    {
        AudioClip GetClip();
        float GetVolume();
    }

    [AssignableTypeFilter(Expanded = true), LabelWidth(60), Serializable]
    public class RandomAudio : IAudio
    {
        public string Name;

        public AudioClip[] Clips;

        [Range(0.0f, 2.0f)]
        public float Volume = 1;    
    
        public AudioClip GetClip() => Clips.GetRandomObject();
        public float GetVolume() => Volume;
    }

    [AssignableTypeFilter(Expanded = true), LabelWidth(60), Serializable]
    public class Audio : IAudio
    {
        public string Name;
    
        public AudioClip Clip;
    
        [Range(0.0f, 2.0f)]
        public float Volume = 1;

        public AudioClip GetClip() => Clip;
        public float GetVolume() => Volume;
    }

}