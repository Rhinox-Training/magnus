using System;
using System.Collections;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    public interface ILevelExitTransitionEffect
    {
        float ExitDuration { get; }
        void TransitionFrom();
    }
    
    public interface ILevelEnterTransitionEffect
    {
        float EnterDuration { get; }
        void TransitionTo();
    }
    
    
    [AssignableTypeFilter]
    public abstract class LevelTransitionEffect
    {
        public abstract float TransitionTime { get; }
        
        public virtual void TransitionFrom()
        {
            
        }
        
        public virtual void TransitionTo()
        {
            
        }
    }

    public class FadeTransition : LevelTransitionEffect
    {
        // Effects
        private IEnumerator FadeScreen(Color color, float duration)
        {
            throw new NotImplementedException();
            // TODO: Implement this, VRTK one used  to work but only with old rendering
            //     
            // foreach (var fader in faders)
            //     fader.Fade(color, duration);
            //
            // while (faders.Any(fade => fade.IsTransitioning()))
            //     yield return null;
        }

        public override float TransitionTime { get; }
    }
}