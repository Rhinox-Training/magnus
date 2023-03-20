using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Rhinox.Magnus.Editor
{
    [Serializable, HideReferenceObjectPicker]
    public abstract class PostBuildStep
    {
        public bool Filter;
        
        [ShowIf("@Filter")]
        public HashSet<BuildTarget> ActiveTargets;

        protected PostBuildStep()
        {
            ActiveTargets = new HashSet<BuildTarget>();
        }

        public bool Execute(BuildTarget target, string buildDirectory, string projectFileName)
        {
            if (!IsStepActiveForTarget(target))
                return true;
            
            return OnExecute(target, buildDirectory, projectFileName);
        }

        private bool IsStepActiveForTarget(BuildTarget target)
        {
            if (!Filter)
                return true;
            if (ActiveTargets == null)
                return false;

            return ActiveTargets.Contains(target);
        }
        
        protected abstract bool OnExecute(BuildTarget target, string buildDirectory, string projectFileName);

#if UNITY_EDITOR
        [ShowInInspector, DisplayAsString, PropertyOrder(-1), HideLabel]
        public string TypeName => this.GetType().Name;
#endif
    }
}