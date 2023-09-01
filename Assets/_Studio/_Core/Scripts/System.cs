using System;
using UnityEngine;
using PlayShifu.Terra;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class System : MonoBehaviour
    {
        private SystemConfigurationSO configData;
        private StudioState previousStudioState;
        private StudioState currentStudioState;
        private Scene currentActiveScene;
        private LoadSceneParameters sceneLoadParameters;

        public SystemConfigurationSO ConfigSO { get { return configData; } }
        public StudioState PreviousStudioState { get { return previousStudioState; } }
        public StudioState CurrentStudioState { get { return currentStudioState; } }

        private void Awake()
        {
            SystemOp.Register(this);
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            configData = (SystemConfigurationSO)SystemOp.Load(ResourceTag.SystemConfig);
            currentStudioState = configData.DefaultStudioState;
            previousStudioState = StudioState.Bootstrap;
            sceneLoadParameters = new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Additive
            };
            LoadSilentServices();
            LoadSubsystemScene();
        }

        private void LoadSilentServices()
        {
            SystemOp.Register(new CrossSceneDataHolder());
            SystemOp.Register(new FileService());
            if (configData.PickupSavedData)
            {
                var shouldIgnore =
#if UNITY_EDITOR
                false;
#else
                true;
#endif
                SystemOp.Resolve<FileService>().WriteFile(
                    configData.SceneDataToLoad.text,
                    FileService.GetSavedFilePath(ConfigSO.SceneDataToLoad.name),
                    shouldIgnore);
            }
        }

        private void LoadSubsystemScene()
        {
            var sceneToLoad = currentStudioState == StudioState.Editor
                ? configData.EditorSceneName
                : configData.RuntimeSceneName;
            SceneManager.LoadSceneAsync(sceneToLoad, sceneLoadParameters);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            currentActiveScene = scene;
            SceneManager.SetActiveScene(scene);
            SystemOp.Resolve<ISubsystem>().Initialize();
        }

        public void SwitchState()
        {
            DisposeCurrentSubSystem(LoadSubsystemScene);
            currentStudioState = GetNextState();
            previousStudioState = GetOtherState();
        }

        private void DisposeCurrentSubSystem(Action onUnloadComplete)
        {
            var subSystem = SystemOp.Resolve<ISubsystem>();
            subSystem?.Dispose();
            var operation = SceneManager.UnloadSceneAsync(currentActiveScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            operation.completed += (result) =>
            {
                GC.Collect(0, GCCollectionMode.Forced);
                onUnloadComplete?.Invoke();
            };
        }

        private StudioState GetNextState()
        {
            var index = (int)currentStudioState;
            var nextIndex = ++index % 3;
            if (nextIndex == 0) nextIndex++;
            return (StudioState)nextIndex;
        }

        private StudioState GetOtherState()
        {
            return currentStudioState switch
            {
                StudioState.Runtime => StudioState.Editor,
                _ => StudioState.Runtime,
            };
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SystemOp.Resolve<ISubsystem>()?.Dispose();
            SystemOp.Unregister<CrossSceneDataHolder>();
            SystemOp.Unregister<FileService>();
            SystemOp.Unregister(this);
            SystemOp.Flush();
            EditorOp.Flush();
            RuntimeOp.Flush();
        }
    }
}
