using System;
using Sirenix.OdinInspector;
using UnityEditor.Build.Reporting;

namespace Rhinox.Magnus.Editor
{
    [Serializable, HideReferenceObjectPicker]
    public abstract class PreBuildStep
    {
        protected PreBuildStep()
        {
        }

        public bool Execute(BuildReport report)
        {
            return OnExecute();
        }
        
        protected abstract bool OnExecute();
        
        public void CleanUp()
        {
            OnCleanUp();
        }

        protected virtual void OnCleanUp() { }

#if UNITY_EDITOR
        [ShowInInspector, DisplayAsString, PropertyOrder(-1), HideLabel]
        public string TypeName => this.GetType().Name;
#endif
    }
}