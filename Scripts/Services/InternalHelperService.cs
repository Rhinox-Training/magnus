using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus
{
    [ServiceLoader(int.MinValue)]
    internal class InternalHelperService : AutoService<InternalHelperService>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
#if !UNITY_EDITOR
            var go = transform.GetOrAddComponent<ServiceAwakerHelper>();
            go.hideFlags = HideFlags.HideAndDontSave;
#endif
        }
    }
}