using System;
using mixpanel;
using UnityEngine;
using PlayShifu.Terra;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class System : MonoBehaviour
    {
        private const int TARGET_FRAME_RATE = 60;
        private SystemConfigurationSO configData;
        private RTDataManagerSO systemData;
        private StudioState previousStudioState;
        private StudioState currentStudioState;
        private Scene currentActiveScene;

        public bool IsSimulating { get; private set; }
        public Func<bool> CanInitiateSubsystemProcess { get; set; }
        public RTDataManagerSO SystemData { get { return systemData; } }
        public SystemConfigurationSO ConfigSO { get { return configData; } }
        public StudioState PreviousStudioState { get { return previousStudioState; } }
        public StudioState CurrentStudioState { get { return currentStudioState; } }

        private const string FIRST_TIME_LAUNCH_PREF_KEY = "ALFT";

        private void Awake()
        {
            SystemOp.Register(this);
            TrackOnStartEvent();
        }

        private void Start()
        {
            Initialize();
            LoadSilentServices();
        }

        private void Initialize()
        {
            Application.targetFrameRate = TARGET_FRAME_RATE;
            SceneManager.sceneLoaded += OnSceneLoaded;
            configData = (SystemConfigurationSO)SystemOp.Load(ResourceTag.SystemConfig);
            systemData = (RTDataManagerSO)SystemOp.Load(ResourceTag.SystemData);
            currentStudioState = configData.DefaultStudioState;
            previousStudioState = StudioState.Bootstrap;
            SystemOp.Resolve<LoginScreenView>().OnLoginSuccessful += LoadSceneData;
        }

        private void LoadSilentServices()
        {
            SystemOp.Register(new CrossSceneDataHolder());
            SystemOp.Register(new FileService());
            SystemOp.Register(new User());
            SystemOp.Register(new SaveSystem());
            SystemOp.Resolve<LoginScreenView>().Init();
        }

        private void LoadSceneData()
        {
            if (configData.PickupSavedData)
            {
                var shouldIgnore = !Helper.IsInUnityEditor();
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
            SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (!CanInitiateSubsystemProcess?.Invoke() ?? false)
            {
                return;
            }
            currentActiveScene = scene;
            SceneManager.SetActiveScene(scene);
            SystemOp.Resolve<ISubsystem>().Initialize(scene);
            if (PreviousStudioState == StudioState.Bootstrap &&
                SystemOp.Resolve<LoginScreenView>())
            {
                SystemOp.Resolve<LoginScreenView>().FuckOff();
            }
        }

        public void SwitchState()
        {
            DisposeCurrentSubSystem(LoadSubsystemScene);
            currentStudioState = GetNextState();
            previousStudioState = GetOtherState();
        }

        public void SetSimulationState(bool isEnabled)
        {
            IsSimulating = isEnabled;
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

        private void TrackOnStartEvent()
        {
            if (!FIRST_TIME_LAUNCH_PREF_KEY.HasKeyInPrefs())
            {
                Mixpanel.Track("AppLaunchFirstTime");
                FIRST_TIME_LAUNCH_PREF_KEY.SetPref(1);
            }
            Mixpanel.Track("AppLaunch");
            Mixpanel.Flush();
        }
    }
}
