#if USING_GRAPHY
using System.Reflection;
using Rhinox.Perceptor;
using Tayx.Graphy;
using UnityEngine;

namespace Rhinox.Magnus
{
    [ServiceLoader()]
    public class GraphyService : AutoService<GraphyService>
    {
        private GameObject _graphyInstance;

        protected override void Start()
        {
            base.Start();

            if (MagnusProjectSettings.Instance.GraphyPrefab)
            {
                _graphyInstance = GameObject.Instantiate(MagnusProjectSettings.Instance.GraphyPrefab, transform, false);
                
                var startupField = typeof(GraphyManager).GetField("m_enableOnStartup",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                startupField.SetValue(GraphyManager.Instance, false);
            }
        }

        public void ToggleGUI()
        {
            if (_graphyInstance == null)
            {
                PLog.Warn<MagnusLogger>("Graphy instance is null");
                return;
            }

            GraphyManager.Instance.ToggleActive();
        }
    }
}
#endif