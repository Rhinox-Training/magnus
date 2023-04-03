using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus
{
    [HideReferenceObjectPicker]
     public abstract class GameModeModifier
     {
         private List<MonoBehaviour> _managersToBeDestroyedAtEndOfFrame;

         public bool IsEnabled { get; private set; }
         
         public void Enable()
         {
             if (IsEnabled)
                 return;
             
             SceneReadyHandler.OnSceneReady(OnSceneReady);

             IsEnabled = true;
             OnEnable();
         }

         private void OnSceneReady()
         {
             if (!IsEnabled)
                 return;

             OnSceneStateChange();
         }

         protected abstract void OnEnable();

         public void Disable()
         {
             if (!IsEnabled)
                 return;
             
             IsEnabled = false;
             SceneReadyHandler.RemoveHandler(OnSceneReady);
             OnDisable();
         }

         protected abstract void OnDisable();

         /// <summary>
         /// Called when the scene is made ready or during OnEnable if the scene is already ready
         /// </summary>
         protected virtual void OnSceneStateChange()
         {
             
         }

         
         // ============================================================================================================
         // Helper Methods
         protected T FindOrCreateManager<T>() where T : MonoBehaviour, ISingleton<T>
         {
             return FindOrCreateManager<T>(out bool created);
         }
         
         protected virtual T FindOrCreateManager<T>(out bool created) where T : MonoBehaviour, ISingleton<T>
         {
             T manager = FindManagerInScene<T>();
             if (manager != null)
             {
                 created = false;
                 return manager;
             }

             created = true;
             GameObject managerObj = new GameObject($"[AUTO-GENERATED] Manager: {typeof(T).Name}");
             return managerObj.AddComponent<T>();
         }
         
         protected T FindManager<T>() where T : MonoBehaviour, ISingleton<T>
         {
             return FindManagerInScene<T>();
         }

         private T FindManagerInScene<T>(bool silenceDebug = true) where T : MonoBehaviour, ISingleton<T>
         {
             if (_managersToBeDestroyedAtEndOfFrame == null)
                 _managersToBeDestroyedAtEndOfFrame = new List<MonoBehaviour>();

             _managersToBeDestroyedAtEndOfFrame = _managersToBeDestroyedAtEndOfFrame.Where(x => x != null).ToList();
             
             var managers = GameObject.FindObjectsOfType<T>();

             if (!silenceDebug && managers.Length > 1)
             {
                 PLog.Warn<MagnusLogger>($"GameModeModifier - Multiple managers detected of type {typeof(T).Name}, warning...");
             }
             
             foreach (var manager in managers)
             {
                 if (_managersToBeDestroyedAtEndOfFrame.Contains(manager))
                     continue;
                 
                 return manager;
             }

             return default(T);
         }
         
         protected void DisposeManager<T>() where T : MonoBehaviour, ISingleton<T>
         {
             T manager = FindManagerInScene<T>(false);
             if (manager == null)
                 return;

             if (manager.gameObject.scene.buildIndex == -1)
             {
                 PLog.Warn<MagnusLogger>($"GameModeModifier - Manager of type {typeof(T).Name} might have DontDestroyOnLoad enabled, will not dispose...");
                 return;
             }

             _managersToBeDestroyedAtEndOfFrame.Add(manager);
             Utility.Destroy(manager.gameObject);
         }
     }
 }