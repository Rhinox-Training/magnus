/*using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace Rhinox.Magnus
{
    [ServiceLoader]
    public class CameraInfo : AutoService<CameraInfo>
    {
        private List<Camera> _activeCameras;
        private List<Camera> _previousFrameCameras;
        
        public IReadOnlyList<Camera> ActiveCameras => _activeCameras.AsReadOnly(); 
        private const string MainCameraTag = "MainCamera";

        public Camera Main => GetMainCamera();

        public delegate void CameraDelegate(Camera cameras);
        
        public event CameraDelegate CameraDisabled;
        public event CameraDelegate NewActiveCamera;

        protected override void Start()
        {
            _previousFrameCameras = new List<Camera>();
            _activeCameras = new List<Camera>();
            Camera.onPostRender += HandleCamera;
#if UNITY_2019_1_OR_NEWER
            RenderPipelineManager.endFrameRendering += OnCameraRendering;
#endif
        }

        protected override void Update()
        {
            var sw = Stopwatch.StartNew();
            // Track whether any cameras were diabled
            for (var i = 0; i < _activeCameras.Count; i++)
            {
                var c = _activeCameras[i];

                if (!_previousFrameCameras.Contains(c))
                    OnCameraDisabled(c);
            }
            
            // Track whether there are any new cameras
            for (var i = 0; i < _previousFrameCameras.Count; i++)
            {
                var c = _previousFrameCameras[i];

                if (!_activeCameras.Contains(c))
                    OnNewActiveCamera(c);
            }
            sw.Stop();
            Debug.Log("[CameraInfo::Update::Events] Took " + sw.Elapsed.TotalMilliseconds.ToString("#.000") + "ms");
            sw.Restart();

            // Swap the list so we don't need to create new ones
            Utility.Swap(ref _activeCameras, ref _previousFrameCameras);
            _previousFrameCameras.Clear();
            
            sw.Stop();
            Debug.Log("[CameraInfo::Update::Swap] Took " + sw.Elapsed.TotalMilliseconds.ToString("#.000") + "ms");
        }

        private void OnNewActiveCamera(Camera c)
        {
            NewActiveCamera?.Invoke(c);
            Debug.Log("Camera Enabled: " + c.name);
        }

        private void OnCameraDisabled(Camera c)
        {
            CameraDisabled?.Invoke(c);
            Debug.Log("Camera Disabled: " + c.name);
        }
        
#if UNITY_2019_1_OR_NEWER
        private void OnCameraRendering(ScriptableRenderContext context, Camera[] cameras)
        {
            for (var i = 0; i < cameras.Length; i++)
                HandleCamera(cameras[i]);
        }
#endif
        
        private void HandleCamera(Camera cam)
        {
            _previousFrameCameras.Add(cam);
        }
        
        private Camera GetMainCamera()
        {
            for (int i = 0; i < _activeCameras.Count; ++i)
            {
                if (_activeCameras[i].CompareTag(MainCameraTag))
                    return _activeCameras[i];
            }

            return null;
        }
    }
}*/