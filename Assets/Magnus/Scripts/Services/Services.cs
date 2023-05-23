using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Utilities.Attributes;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus
{
    public static class Services
    {

        public static T FindService<T>() where T : IService
        {
            return ServiceInitiator.ActiveServices.OfType<T>().FirstOrDefault();
        }

        public static IService FindService(string name)
        {
            return ServiceInitiator.ActiveServices.FirstOrDefault(x =>
                x.GetType().Name.Equals(name, StringComparison.InvariantCulture));
        }

#if !UNITY_EDITOR
        private static List<Type> _availableServiceTypes;
#endif
        public static IEnumerable<Type> GetAvailableServices()
        {
#if UNITY_EDITOR
            foreach (var type in TypeCache.GetTypesWithAttribute<ServiceLoaderAttribute>())
            {
                if (!type.InheritsFrom(typeof(AutoService<>)) || type.IsAbstract || type.IsGenericTypeDefinition)
                    continue;

                if (type == typeof(InternalHelperService)) // NOTE: Invisible for developers
                    continue;

                yield return type;
            }
#else
            if (_availableServiceTypes == null)
            {
                _availableServiceTypes = new List<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.InheritsFrom(typeof(AutoService<>)) || type.IsAbstract ||
                            type.IsGenericTypeDefinition)
                            continue;

                        var serviceTypeAttribute =
                            CustomAttributeExtensions.GetCustomAttribute<ServiceLoaderAttribute>(type);
                        if (serviceTypeAttribute == null)
                            continue;

                        if (type == typeof(InternalHelperService)) // NOTE: Invisible for developers
                            continue;

                        _availableServiceTypes.Add(type);
                    }
                }
            }
            return _availableServiceTypes;
#endif
        }
    }

    [ExecutionOrder(-9999), InitializationHandler]
    internal static class ServiceInitiator
    {
        private static List<IService> _activeServices;
        private static bool _servicesWaked;
        internal static IReadOnlyCollection<IService> ActiveServices => _activeServices;

        [OrderedRuntimeInitialize(-9999)] // Only called once
        private static void OnRuntimeMethodLoad()
        {
            Application.quitting += OnQuitting;

            List<Type> types = new List<Type>()
            {
                typeof(InternalHelperService)
            };
            foreach (var type in Services.GetAvailableServices())
            {
                if (!ShouldLoadService(type))
                    continue;
                types.Add(type);
            }

            types.SortBy(x => CustomAttributeExtensions.GetCustomAttribute<ServiceLoaderAttribute>(x).LoadOrder);

            var serviceLoad = new List<IService>();
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                var service = CreateService(type);
                if (service != null)
                    serviceLoad.Add(service);
            }

            _activeServices = serviceLoad.ToList();
#if UNITY_EDITOR
            UnloadService<InternalHelperService>();
            AwakeServices();
#endif
        }

        private static bool ShouldLoadService(Type t)
        {
            if (MagnusProjectSettings.Instance.ServiceSettings == null)
                return true;
            return MagnusProjectSettings.Instance.ServiceSettings.ShouldLoadService(t);
        }

        private static void OnQuitting()
        { 
            for (var i = 0; i < _activeServices.Count; i++)
            {
                var service = _activeServices[i];
                var type = service.GetType();

                if (_servicesWaked)
                {
                    InvokeSimpleMethod(type, "OnDisable", service);
                    InvokeSimpleMethod(type, "OnDestroy", service);
                }
            }

            _activeServices.Clear();
            _servicesWaked = false;
        }

        private static IService CreateService(Type type)
        {
            MethodInfo m = type.GetMethod("Initialize",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (m == null)
                return null;

            var service = m.Invoke(null, null) as IService;
            return service;
        }

        internal static void AwakeServices()
        {
            if (_servicesWaked)
                return;
            
            _servicesWaked = true;
            // Execute the Awake of all the services
            for (var i = 0; i < _activeServices.Count; i++)
            {
                var service = _activeServices[i];
                var type = service.GetType();

                if (type == typeof(InternalHelperService))
                    continue;

                try
                {
                    InvokeSimpleMethod(type, "Awake", service);
                    InvokeSimpleMethod(type, "OnEnable", service);
                }
                catch (Exception e)
                {
                    PLog.Error<MagnusLogger>(e.ToString());
                }
            }
        }

        internal static bool UnloadService<T>() where T : AutoService<T>, new()
            => UnloadService(typeof(T));
        
        
        internal static bool UnloadService(Type t)
        {
            bool unloaded = false;
            for (var i = 0; i < _activeServices.Count; i++)
            {
                var service = _activeServices[i];
                var type = service.GetType();

                if (type != t)
                    continue;

                if (_servicesWaked)
                {
                    InvokeSimpleMethod(type, "OnDisable", service);
                    InvokeSimpleMethod(type, "OnDestroy", service);
                }

                _activeServices.Remove(service);
                unloaded = true;
                break; // Have to break, in order not to get an exception
            }

            return unloaded;
        }

        private static void InvokeSimpleMethod(Type type, string methodName, IService service)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                return;
            MethodInfo m = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m != null)
            {
                PLog.Trace<MagnusLogger>($"[AutoService] {methodName} {type.Name}");
                m.Invoke(service, Array.Empty<object>());
            }
        }
    }
}