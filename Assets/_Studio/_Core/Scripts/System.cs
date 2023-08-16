using System;
using UnityEngine;
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
            LoadSubsystemScene();
            SystemOp.Register(new CrossSceneDataHolder());
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
            SystemOp.Unregister(this);
            SystemOp.Flush();
            EditorOp.Flush();
            RuntimeOp.Flush();
        }
    }
}
