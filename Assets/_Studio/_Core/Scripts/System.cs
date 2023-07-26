using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class System : MonoBehaviour
    {
        private const string RESOURCE_CONFIGURATION_PATH = "SystemSettings";
        private SystemConfigurationSO configData;
        private StudioState currentStudioState;
        private Scene currentActiveScene;
        private LoadSceneParameters sceneLoadParameters;

        private void Awake()
        {
            Interop<SystemInterop>.Current.Register(this);
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
            sceneLoadParameters = new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D
            };
            LoadSubsystemScene();
            Interop<SystemInterop>.Current.Register(new CrossSceneDataHolder());
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
            Interop<SystemInterop>.Current.Resolve<ISubsystem>().Initialize();
        }

        public void SwitchState()
        {
            //Get the state of objects and save them under a common parent with no editor references
            Debug.Log($"Switching state");
            DisposeCurrentSubSystem(LoadSubsystemScene);
            currentStudioState = GetNextState();
        }

        private void DisposeCurrentSubSystem(Action onUnloadComplete)
        {
            var subSystem = Interop<SystemInterop>.Current.Resolve<ISubsystem>();
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
            var nextIndex = ++index % 2;
            return (StudioState)index;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Interop<SystemInterop>.Current.Resolve<ISubsystem>()?.Dispose();
            Interop<SystemInterop>.Current.Unregister(this);
            Interop<SystemInterop>.Current.Unregister<CrossSceneDataHolder>();
            Interop<SystemInterop>.Flush();
            Interop<EditorInterop>.Flush();
            Interop<RuntimeInterop>.Flush();
        }
    }
}
