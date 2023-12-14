using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus
{
    [ServiceLoader(-4000)]
    public class LevelLoader : AutoService<LevelLoader>
    {
        public enum LoadState
        {
            MainScene,
            SubScene,
        }
        
        private static string _scenePathTransitionTarget = null;
        private static LevelDefinitionData _sceneDataTransitionTarget = null;

        public delegate void SceneLoadEventHandler(LevelLoader sender, SceneReferenceData loadedScene, LoadState state);

        public static event SceneLoadEventHandler LevelLoaded;

        private static readonly Vector3 _offworldLocation = new Vector3(2000,-2000,2000);
        //private ManagedCoroutine _runningLoad;
        private bool _runningLoad = false;

        public bool IsLoadingNewScene => _runningLoad;
        
        private LevelLoadingArea _activeArea;
        
        private LevelDefinitionData _activeSceneData;
        private string _activeScenePath;
        private List<SceneReferenceData> _additivelyLoadedScenes;
        
        private static readonly YieldInstruction _awaiter = new WaitForSeconds(0.1f);
        
        private void RefreshActiveScene()
        {
            _activeScenePath = SceneManager.GetActiveScene().path;
        }
        
        protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            base.OnSceneLoaded(scene, mode);
            RefreshActiveScene();

            // If it's not the main scene, no need to do anything for it, just load it
            // TODO is this correct?
            if (SceneManager.GetActiveScene() != scene)
                return;

            if (!IsConfiguredAndEnabled())
            {
                PLog.Warn<MagnusLogger>("LevelLoader not configured in Project Settings");
                return;
            }

            var blocker = Object.FindObjectsOfType<LevelLoaderBlocker>()
                .Where(x => x.BlockLevelLoader)
                .FirstOrDefault(x => x.gameObject.scene == scene);

            if (blocker)
            {
                PLog.Info<MagnusLogger>("LevelLoader blocked...", blocker);
                return;
            }

            if (!InitiateTransition())
                PLog.Info<MagnusLogger>("[OnSceneLoaded] Already loaded transition");
        }
        
        protected override void OnSceneUnloaded(Scene newScene)
        {
            base.OnSceneUnloaded(newScene);
            RefreshActiveScene();
        }

        public static string GetActiveScene()
        {
            if (Instance.IsLoadingNewScene)
                return _scenePathTransitionTarget;
            return Instance._activeScenePath;
        }

        public static bool IsSceneActive(Scene scene)
        {
            return scene.path.Equals(GetActiveScene());
        }

        public LevelDefinitionData GetActiveLevelDefinition()
        {
            if (IsLoadingNewScene)
                return _sceneDataTransitionTarget;
            return _activeSceneData;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _additivelyLoadedScenes = new List<SceneReferenceData>();
            if (!IsConfiguredAndEnabled())
            {
                PLog.Warn<MagnusLogger>("LevelLoader not configured in Project Settings");
                return;
            }
            
            // Note: InitiateTransition used to be here
            // Removed due to switching to ManagedCoroutine: Manager will get destroyed before completing
            
            
            // If configured we want the SceneReadyHandler to await this. Signal it that things will have to wait.
            if (IsConfiguredAndEnabled())
                SceneReadyHandler.Instance.Disable(); // NOTE: SceneReadyHandler service needs to be loaded before this service
        }
        
        private bool IsConfiguredAndEnabled()
        {
            return MagnusProjectSettings.Instance.LoadingScenePrefab != null; // TODO: more complete check needed
        }

        private bool InitiateTransition()
        {
            if (_scenePathTransitionTarget != null || _runningLoad)
                return false;
            
            PLog.Info<MagnusLogger>("[OnSceneLoaded] LevelLoader first run, start up initial scene transition");
            _runningLoad = true;
            ManagedCoroutine.Begin(HandleLoading());

            return true;
        }


        // [Button(ButtonSizes.Medium)]
        // public void ReloadLevel() => LoadScene(SceneManager.GetActiveScene().buildIndex);
        //
        // // Instant scene loading
        //public void LoadScene(int index) => StartCoroutine(LoadLevelAsync(index));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneRequestString">Scene name or path</param>
        public void LoadScene(string sceneRequestString)
        {
            if (!IsConfiguredAndEnabled())
            {
                PLog.Warn<MagnusLogger>("LevelLoader not configured correctly in Project Settings");
                return;
            }
            
            if (_runningLoad)
            {
                PLog.Error<MagnusLogger>($"Can't transition to new scene, load is still running...");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(sceneRequestString))
            {
                PLog.Error<MagnusLogger>($"Can't transition to new scene, scene request string was empty...");
                return;
            }

            string scenePath;
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneRequestString);
            if (buildIndex == -1)
                scenePath = GetScenePathByName(sceneRequestString);
            else
                scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            
            if (scenePath == null)
            {
                PLog.Error<MagnusLogger>($"Can't transition to new scene, scene '{sceneRequestString}' was not found...");
                return;
            }
            
            _runningLoad = true;
            ManagedCoroutine.Begin(LoadLevelAsync(scenePath));
        }
        
        public void LoadScene(LevelDefinitionData scene)
        {
            if (!IsConfiguredAndEnabled())
            {
                PLog.Warn<MagnusLogger>("LevelLoader not configured correctly in Project Settings");
                return;
            }
            if (_runningLoad)
            {
                PLog.Error<MagnusLogger>($"Can't transition to new scene, load is still running...");
                return;
            }

            if (scene == null)
            {
                PLog.Error<MagnusLogger>($"Can't transition to new scene, LevelDefinitionData is null...");
                return;
            }
            
            _runningLoad = true;
            ManagedCoroutine.Begin(LoadLevelAsync(scene));
        }

        public void LoadScene(int buildIndex)
        {
            if (!IsConfiguredAndEnabled())
            {
                PLog.Warn<MagnusLogger>("LevelLoader not configured correctly in Project Settings");
                return;
            }
            if (_runningLoad)
            {
                PLog.Error<MagnusLogger>($"Can't transition to new scene, load is still running...");
                return;
            }

            var scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                PLog.Error<MagnusLogger>($"Can't transition to new scene, scene at index {buildIndex} does not exist...");
                return;
            }
            
            _runningLoad = true;
            var coroutine = ManagedCoroutine.Begin(LoadLevelAsync(scenePath));
        }
        
        public void ReloadScene()
        {
            if (!IsConfiguredAndEnabled())
            {
                PLog.Warn<MagnusLogger>("LevelLoader not configured correctly in Project Settings");
                return;
            }
            if (_runningLoad)
            {
                PLog.Error<MagnusLogger>($"Can't transition to new scene, load is still running...");
                return;
            }
            _runningLoad = true;
            var coroutine = ManagedCoroutine.Begin(ReloadLevelAsync());
        }
        
        // TODO: what if not enabled loadingscene prefab
        private IEnumerator HandleLoading(bool skipEnterTransition = false)
        {
            PLog.Trace<MagnusLogger>($"[{nameof(LevelLoader)}] HandleLoading - Started");
            SceneReadyHandler.Instance.Disable();
            
            string scenePath = SceneManager.GetActiveScene().path;
            StartTransitionTo(scenePath);
            
            // Go to the loadingScene
            yield return GoToLevelLoadingArea(skipEnterTransition, false);

            // const int fakeIterationCount = 4;
            // const float time = 0.5f;

            // yield return DoFakeIterations(LoadingStage.Initializing, time, fakeIterationCount);

            yield return ExecuteLoadHandlers(GetSceneLoaders());
            
            // yield return DoFakeIterations(LoadingStage.CleaningUp, time, fakeIterationCount);
            
            yield return RespawnPlayerAndCleanUpArea(null, skipEnterTransition);

            PLog.Trace<MagnusLogger>("HandleLoading - Level loading finished");
            
            FinishTransitionTo();
            
            SceneReadyHandler.Instance.Enable();

            _runningLoad = false;
            _scenePathTransitionTarget = null;
        }

        private IEnumerator DoFakeIterations(LoadingStage stage, float waitTime, int iterations)
        {
            float timing = waitTime / iterations;
            
            for (int i = 0; i < iterations; ++i)
            {
                _activeArea.HandleProgress(stage, 1, 1, i/(float)iterations);
                yield return new WaitForSeconds(timing);
            }
            _activeArea.HandleProgress(stage, 1, 1, 1.0f);
        }

        private IEnumerator GoToLevelLoadingArea(bool skipTransition, bool changingActiveScene)
        { 
            LevelLoadingArea area = Object.FindObjectsOfType<LevelLoaderAreaOverride>()
                .FirstOrDefault(x => x.gameObject.scene.path.Equals(_scenePathTransitionTarget))
                ?.Area;
            
            if (area == null)
                area = MagnusProjectSettings.Instance.LoadingScenePrefab;
            
            // Attempts instantiate even if are is still null?
            if (_activeArea == null)
                _activeArea = Object.Instantiate(area, _offworldLocation, Quaternion.identity, transform);
            
            if (!skipTransition)
                yield return HandleExitTransition();
            
            yield return null;

            if (changingActiveScene && !PlayerManager.Instance.ActivePlayer.IsPlayerPersistent())
                PlayerManager.Instance.KillLocalPlayer();

            if (PlayerManager.Instance.RespawnLocalPlayer(_activeArea.transform.position))
                // TODO: make optional, you might want to keep modifiers on level transition (Project Settings?)
                PlayerManager.Instance.ActivePlayer.ClearAllModifiers(); 

            // make sure the camera is ok?
            yield return new WaitForSeconds(1f);
        }

        private IEnumerator RespawnPlayerAndCleanUpArea(GuidAsset playerStartIdentifier, bool skipTransition)
        {
            PlayerStart playerStart = PlayerStart.FindInCurrentScene(playerStartIdentifier);
            PlayerManager.Instance.RespawnPlayer(playerStart);
            
            if (!skipTransition)
                yield return HandleEnterTransition();
            
            yield return null;

            Utility.Destroy(_activeArea.gameObject);
            _activeArea = null;

            yield return null;
        }
        
        private IEnumerator MarkSingletonsAsDestroying()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            foreach (var rootObj in activeScene.GetRootGameObjects())
            {
                var singletons = rootObj.GetComponentsInChildren<ISingleton>();
                foreach (var singleton in singletons)
                    singleton.IsDestroying = true;
            }

            yield return null;
        }

        private IEnumerator LoadNewLevelAsync(string scenePath)
        {
            // Begin to load the Scene that was specified
            _activeArea.HandleProgress(LoadingStage.Initializing, 1, 1, 0.0f);
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            
            PLog.Debug<MagnusLogger>($"Level load triggered: {scenePath}");
            //When the load is still in progress, output the Text and progress bar
            while (!asyncOperation.isDone)
            {
                _activeArea.HandleProgress(LoadingStage.Initializing, 1, 1, asyncOperation.progress);

                // Check if the load has finished
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }

                yield return _awaiter;
            }
            _activeArea.HandleProgress(LoadingStage.Initializing, 1, 1, 1.0f);
        }
        
        private IEnumerator LoadAdditiveLevelsAsync(SceneReferenceData[] datas)
        {
            if (datas.IsNullOrEmpty())
                yield break;
            
            int stages = datas.Length;
            // Begin to load the Scene that was specified
            _activeArea.HandleProgress(LoadingStage.LoadingStage, 1, stages, 0.0f);

            for (var i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(data.ScenePath, LoadSceneMode.Additive);
                asyncOperation.allowSceneActivation = false;

                PLog.Debug<MagnusLogger>($"Additive Level Load [{i + 1}/{stages}] triggered: {data.ScenePath}");
                //When the load is still in progress, output the Text and progress bar
                while (!asyncOperation.isDone)
                {
                    _activeArea.HandleProgress(LoadingStage.LoadingStage, i+1, stages, asyncOperation.progress);

                    // Check if the load has finished
                    if (asyncOperation.progress >= 0.9f)
                        asyncOperation.allowSceneActivation = true;

                    yield return _awaiter;
                }
                
                LevelLoaded?.Invoke(this, data, LoadState.SubScene);
                
                _additivelyLoadedScenes.Add(data);

                _activeArea.HandleProgress(LoadingStage.LoadingStage, i+1, stages, 1.0f);
            }
        }
        
        private IEnumerator ReloadActiveLevelAsync()
        {
            string sceneName = SceneManager.GetActiveScene().path;
            // Begin to load the Scene that was specified
            _activeArea.HandleProgress(LoadingStage.Initializing, 1, 1, 0.0f);
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single); // TODO: additive loaded scenes will be removed
            asyncOperation.allowSceneActivation = false;
            
            PLog.Debug<MagnusLogger>($"Level load triggered: {sceneName}");
            //When the load is still in progress, output the Text and progress bar
            while (!asyncOperation.isDone)
            {
                _activeArea.HandleProgress(LoadingStage.Initializing, 1, 1, asyncOperation.progress);

                // Check if the load has finished
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }

                yield return _awaiter;
            }
            _additivelyLoadedScenes.Clear(); // NOTE: LoadSceneMode is set to Single, this unloads the additively loaded scenes
            _activeArea.HandleProgress(LoadingStage.Initializing, 1, 1, 1.0f);
        }

        private IEnumerator UnloadActiveLevelAsync(float delay = 0.0f)
        {
            if (SceneReadyHandler.Instance.IsEnabled)
                SceneReadyHandler.Instance.Disable();
            
            Scene oldScene = SceneManager.GetActiveScene();
            if (delay > float.Epsilon)
                yield return new WaitForSeconds(delay);
            
            _activeArea.HandleProgress(LoadingStage.CleaningUp, 1, 1, 0.0f);
            var asyncOperation = SceneManager.UnloadSceneAsync(oldScene);
            while (!asyncOperation.isDone)
            {
                _activeArea.HandleProgress(LoadingStage.CleaningUp, 1, 1, asyncOperation.progress);

                // Check if the load has finished
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }
            _activeArea.HandleProgress(LoadingStage.CleaningUp, 1, 1, 1.0f);
        }

        private IEnumerator UnloadAdditivelyLoadedScenes()
        {
            if (_additivelyLoadedScenes.IsNullOrEmpty())
                yield break;
            
            int stages = _additivelyLoadedScenes.Count;
            _activeArea.HandleProgress(LoadingStage.CleaningUp, 1, stages, 0.0f);

            for (var i = 0; i < _additivelyLoadedScenes.Count; i++)
            {
                var scene = _additivelyLoadedScenes[i];
                _activeArea.HandleProgress(LoadingStage.CleaningUp, i+1, stages, 0.0f);
                var asyncOperation = SceneManager.UnloadSceneAsync(scene.ScenePath);
                PLog.Debug<MagnusLogger>($"Additive Level Unload [{i + 1}/{stages}] triggered: {scene.ScenePath}");
                while (!asyncOperation.isDone)
                {
                    _activeArea.HandleProgress(LoadingStage.CleaningUp, i+1, stages, asyncOperation.progress);

                    // Check if the load has finished
                    if (asyncOperation.progress >= 0.9f)
                    {
                        asyncOperation.allowSceneActivation = true;
                    }

                    yield return null;
                }

                _activeArea.HandleProgress(LoadingStage.CleaningUp, i+1, stages, 1.0f);
            }

            _additivelyLoadedScenes.Clear();
        }

        private IEnumerator SetLevelActive(string scenePath)
        {
            var scene = SceneManager.GetSceneByPath(scenePath);
            SceneManager.SetActiveScene(scene);

            yield return null;
        }

        private IEnumerator ExecuteLoadHandlers(ICollection<ILevelLoadHandler> loaders)
        {
            yield return new WaitForSeconds(0.5f); // Wait one frame?
            
            if (loaders == null)
                yield break;

            loaders = SortHandlers(loaders);
            
            int stageIndex = 1;
            foreach (var loading in loaders)
            {
                var title = loading.GetType().GetCSharpName().SplitCamelCase();
                
                _activeArea.HandleProgress(LoadingStage.LoadingStage, stageIndex, loaders.Count, 0.0f, title);
                var loadRoutine = loading.OnLevelLoad();
                while (loadRoutine.MoveNext())
                {
                    float progress = loadRoutine.Current;
                    _activeArea.HandleProgress(LoadingStage.LoadingStage, stageIndex, loaders.Count, progress, title);
                    yield return progress;
                }
                
                _activeArea.HandleProgress(LoadingStage.LoadingStage, stageIndex, loaders.Count, 1.0f, title);
                stageIndex++;
                yield return null;
            }
            
            yield return null;
        }

        private ICollection<ILevelLoadHandler> SortHandlers(ICollection<ILevelLoadHandler> handlers)
        {
            var sortedList = new List<ILevelLoadHandler>();
            foreach (var handler in handlers)
            {
                int insertIndex = FindInsertIndex(sortedList, handler);
                sortedList.Insert(insertIndex, handler);
            }

            return sortedList;
        }

        private int FindInsertIndex(List<ILevelLoadHandler> sortedList, ILevelLoadHandler handler)
        {
            int resultIndex = sortedList.Count; // Insert last as fallback
            for (int index = 0; index < sortedList.Count; ++index)
            {
                var entry = sortedList[index];
                if (handler.LoadOrder >= entry.LoadOrder)
                    continue;
                resultIndex = index;
                break;
            }
            return resultIndex;
        }
        
        private IEnumerator LoadLevelAsync(string scenePath, bool skipExitTransition = false, bool skipEnterTransition = false, float delayTransitionSeconds = 0.0f)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                PLog.Trace<MagnusLogger>($"ERROR: Can't transition to new scene, scenePath '{scenePath}' is empty...");
                yield break;
            }
            
            PLog.Trace<MagnusLogger>($"LoadLevelAsync - Started load of scene {scenePath}");
            if (SceneReadyHandler.Instance.IsEnabled)
                SceneReadyHandler.Instance.Disable();
            
            StartTransitionTo(scenePath);
            
            yield return GoToLevelLoadingArea(skipExitTransition, true);

            yield return MarkSingletonsAsDestroying();
            
            // We must always have an active scene so load the new one first (before unloading)
            yield return LoadNewLevelAsync(scenePath);

            yield return UnloadActiveLevelAsync(delayTransitionSeconds);

            yield return UnloadAdditivelyLoadedScenes();

            yield return SetLevelActive(scenePath);
            
            yield return ExecuteLoadHandlers(GetSceneLoaders());
            
            yield return RespawnPlayerAndCleanUpArea(null, skipEnterTransition);
            
            FinishTransitionTo();
            
            SceneReadyHandler.Instance.Enable();
            
            PLog.Trace<MagnusLogger>("LoadLevelAsync - Finished");
        }

        private void StartTransitionTo(LevelDefinitionData data)
        {
            _sceneDataTransitionTarget = data;
            _scenePathTransitionTarget = data.Scene.ScenePath;
        }
        
        private void StartTransitionTo(string scenePath)
        {
            _sceneDataTransitionTarget = null;
            _scenePathTransitionTarget = scenePath;
        }

        private void FinishTransitionTo()
        {
            if (!_runningLoad)
                return;
            
            _activeSceneData = _sceneDataTransitionTarget;
            _sceneDataTransitionTarget = null;
            
            _activeScenePath = _scenePathTransitionTarget;
            _scenePathTransitionTarget = null;
            
            LevelLoaded?.Invoke(this, new SceneReferenceData(_activeScenePath), LoadState.MainScene);

            _runningLoad = false;
        }
        
        
        private IEnumerator LoadLevelAsync(LevelDefinitionData data)
        {
            var scenePath = data.Scene.ScenePath;

            PLog.Trace<MagnusLogger>($"LoadLevelAsync - Started load of scene {scenePath}");
            if (SceneReadyHandler.Instance.IsEnabled)
                SceneReadyHandler.Instance.Disable();
            
            StartTransitionTo(data);
            
            yield return GoToLevelLoadingArea(true, true);
            
            yield return MarkSingletonsAsDestroying();

            // We must always have an active scene so load the new one first (before unloading)
            yield return LoadNewLevelAsync(scenePath);
            
            yield return UnloadAdditivelyLoadedScenes();

            yield return UnloadActiveLevelAsync(0.0f);

            yield return SetLevelActive(scenePath);

            yield return LoadAdditiveLevelsAsync(data.AdditionalScenes);

            // Do data's LoadHandlers first; might want to spawn/activate some scene handlers
            yield return ExecuteLoadHandlers(GetSceneLoaders(data.LoadHandlers));
            
            yield return RespawnPlayerAndCleanUpArea(data.GetPlayerStartIdentifier(), true);
            
            FinishTransitionTo();
            
            SceneReadyHandler.Instance.Enable();
            
            PLog.Trace<MagnusLogger>("LoadLevelAsync - Finished");
        }
        
        private IEnumerator ReloadLevelAsync(bool skipExitTransition = false, bool skipEnterTransition = false, float delayTransitionSeconds = 0.0f)
        {
            var activeScene = SceneManager.GetActiveScene();
            PLog.Trace<MagnusLogger>($"LoadLevelAsync - Started load of scene {activeScene.name}");
            if (SceneReadyHandler.Instance.IsEnabled)
                SceneReadyHandler.Instance.Disable();

            if (_activeSceneData != null)
                StartTransitionTo(_activeSceneData);
            else if (_activeScenePath != null)
                StartTransitionTo(_activeScenePath);
            else
                StartTransitionTo(activeScene.path);
            
            yield return GoToLevelLoadingArea(skipExitTransition, true);
            
            yield return MarkSingletonsAsDestroying();

            // We must always have an active scene so load the new one first (before unloading)
            yield return ReloadActiveLevelAsync();

            // The reloadActiveScene unloaded the additive scenes as well, reload them next
            if (_activeSceneData != null && _activeSceneData.AdditionalScenes != null)
                yield return LoadAdditiveLevelsAsync(_activeSceneData.AdditionalScenes.ToArray());
            
            yield return ExecuteLoadHandlers(GetSceneLoaders());
            
            yield return RespawnPlayerAndCleanUpArea(null, skipEnterTransition);
            
            FinishTransitionTo();
            
            SceneReadyHandler.Instance.Enable();
            
            PLog.Trace<MagnusLogger>("LoadLevelAsync - Finished");
        }
        
        // private IEnumerator LoadLevelAsync(LevelDefinitionData data)
        // {
        //     var scenePath = data.Scene.ScenePath;
        //     PLog.Trace<MagnusLogger>($"LoadLevelAsync - Started load of scene {scenePath}");
        //     if (SceneReadyHandler.Instance.IsEnabled)
        //         SceneReadyHandler.Instance.Disable();
        //     
        //     _sceneTransitionTarget = scenePath;
        //     
        //     yield return GoToLevelLoadingArea(true, true);
        //
        //     // We must always have an active scene so load the new one first (before unloading)
        //     yield return LoadNewLevelAsync(scenePath);
        //
        //     yield return UnloadActiveLevelAsync(0.0f);
        //     
        //     yield return ExecuteLoadHandlers( data.LoadHandlers );
        //
        //     yield return ExecuteLoadHandlers( GetSceneLoaders() );
        //     
        //     yield return RespawnPlayerAndCleanUpArea(data.GetPlayerStartIdentifier(), true);
        //     
        //     _runningLoad = false;
        //     _sceneTransitionTarget = null;
        //     
        //     SceneReadyHandler.Instance.Enable();
        //     
        //     PLog.Trace<MagnusLogger>("LoadLevelAsync - Finished");
        // }

        private IEnumerator HandleEnterTransition()
        {
            if (_activeArea == null || _activeArea.EnterTransitionEffects == null) 
                yield break;
            
            //float maxExitDuration = levelLoadingArea.EnterTransitionEffects.Max(x => x.EnterDuration);
            // TODO:
        }

        private IEnumerator HandleExitTransition()
        {
            if (_activeArea == null || _activeArea.ExitTransitionEffects == null) 
                yield break;
            
            //float maxExitDuration = levelLoadingArea.ExitTransitionEffects.Max(x => x.ExitDuration);
            // TODO:
        }

        private ICollection<ILevelLoadHandler> GetSceneLoaders(ICollection<ILevelLoadHandler> appendSet = null)
        {
            List<ILevelLoadHandler> list = new List<ILevelLoadHandler>();
            
            Utility.FindSceneObjectsOfTypeAll(list);
            if (appendSet != null)
                list.AddRange(appendSet);
            return list;
        }
        
        // TODO: port to Rhinox.Utilities
        public static string GetScenePathByName(string name)
        {
            int i = 0;
            string sceneName = "FOOBAR";
            while (!string.IsNullOrEmpty(sceneName))
            {
                var scenePathCandidate = SceneUtility.GetScenePathByBuildIndex(i++);
                sceneName = GetNameFromScenePath(scenePathCandidate);
                if (sceneName != null && sceneName.Equals(name, StringComparison.InvariantCulture))
                    return sceneName;
            }

            return null;
        }

        private static string GetNameFromScenePath(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
                return null;
            scenePath = scenePath.Trim();
            const string extension = ".unity";
            if (!scenePath.EndsWith(extension))
                return null;
            
            int indexOf = Math.Max(scenePath.LastIndexOf("/"), scenePath.LastIndexOf("\\"));
            if (indexOf != -1)
                scenePath = scenePath.Substring(indexOf + 1);

            string sceneName = scenePath.Substring(0, scenePath.Length - extension.Length);
            return sceneName;
        }
    }
}