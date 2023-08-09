using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class System : MonoBehaviour
    {
        private const string RESOURCE_CONFIGURATION_PATH = "SystemSettings";
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
            configData = Resources.Load<SystemConfigurationSO>(RESOURCE_CONFIGURATION_PATH);
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
            //Show a loading screen may be
            var sceneToLoad = currentStudioState == StudioState.Editor
                ? configData.EditorSceneName
                : configData.RuntimeSceneName;
            SceneManager.LoadSceneAsync(sceneToLoad, sceneLoadParameters);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            // Debug.Log($"Loaded scene: {scene.name}");
            currentActiveScene = scene;
            SceneManager.SetActiveScene(scene);
            SystemOp.Resolve<ISubsystem>().Initialize();
        }

        public void SwitchState()
        {
            //Get the state of objects and save them under a common parent with no editor references
            // Debug.Log($"Switching state");
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
            SystemOp.Unregister(this);
            SystemOp.Unregister<CrossSceneDataHolder>();
            SystemOp.Flush();
            EditorOp.Flush();
            RuntimeOp.Flush();
        }
    }
}
