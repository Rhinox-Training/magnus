using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus
{
    public abstract class AutoService<T> : IService where T : AutoService<T>, new()
    {
        private static T _instance;
        public static T Instance => _instance;

        private bool _started;
        public bool IsActive => _started;

        private GameObject _generatedRoot;

        protected Transform transform
        {
            get
            {
                if (_generatedRoot is null)
                    _generatedRoot = new GameObject($"[GENERATED] {GetType().Name} Root");
                Object.DontDestroyOnLoad(_generatedRoot);
                return _generatedRoot.transform;
            }
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Execute Start (Awake / OnEnable is handled by ServiceInitiator)
            if (!_started)
            {
                PLog.Trace<MagnusLogger>($"[AutoService] Start {typeof(T).Name}");
                Start();
                _started = true;
            }
            else
            {
                OnNewSceneLoaded(scene, mode);
            }

            TickManager.Instance.Tick -= Tick;
            TickManager.Instance.Tick += Tick;
        }

        protected virtual void OnNewSceneLoaded(Scene newScene, LoadSceneMode mode)
        {
            
        }
        
        protected virtual void OnSceneUnloaded(Scene newScene)
        {
            
        }

        protected static IService Initialize()
        {
            if (_instance != null)
            {
                PLog.Warn<MagnusLogger>($"Initialize failed for service {typeof(T).Name}, already exists.");
                return _instance;
            }

            _instance = new T();
            
            PLog.Info<MagnusLogger>($"Initializing service {typeof(T).Name}");
            SceneManager.sceneLoaded += _instance.OnSceneLoaded;
            SceneManager.sceneUnloaded += _instance.OnSceneUnloaded;
            
            _instance.OnInitialize();
            PLog.Trace<MagnusLogger>($"Initializing service {typeof(T).Name} completed");
            return _instance;
        }

        protected virtual void OnInitialize() { }

        protected virtual void Awake() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() { }
        
        protected virtual void Start() { }
        
        protected virtual void Update() { }
        
        private void Tick()
        {
            Update();
        }
        
        public virtual void DumpInformation(IInformationDump data)
        {
            data.WriteLine($"Service '{GetType().Name}'");

            var memberInfos = SerializeHelper.GetPublicAndSerializedMembers(GetType());
            foreach (var memberInfo in memberInfos)
            {
                if (memberInfo == null)
                    continue;

                var val = memberInfo.GetValue(this);
                data.WriteLine($"    {memberInfo.Name}: {val}");
            }

        }
    }
}