using System;
using mixpanel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class System : MonoBehaviour
    {
        private const int TARGET_FRAME_RATE = 60;
        private SystemConfigurationSO configData;
        private RTDataManagerSO systemData;
        private StudioState defaultStudioState;
        private StudioState previousStudioState;
        private StudioState currentStudioState;
        private Scene currentActiveScene;

        public event Action OnSubsystemLoaded;
        public bool IsSimulating { get; private set; }
        public Func<bool> CanInitiateSubsystemProcess { get; set; }
        public RTDataManagerSO SystemData => systemData;
        public SystemConfigurationSO ConfigSO => configData;
        public StudioState PreviousStudioState => previousStudioState;
        public StudioState CurrentStudioState => currentStudioState;
        public StudioState DefaultStudioState => defaultStudioState;

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
            AutoloadSceneIfSetFromConfig();
        }

        private void Initialize()
        {
            Application.targetFrameRate = TARGET_FRAME_RATE;
            SceneManager.sceneLoaded += OnSceneLoaded;
            configData = (SystemConfigurationSO)SystemOp.Load(ResourceTag.SystemConfig);
            systemData = (RTDataManagerSO)SystemOp.Load(ResourceTag.SystemData);
            defaultStudioState = currentStudioState = configData.DefaultStudioState;
            previousStudioState = StudioState.Bootstrap;
        }

        private void LoadSilentServices()
        {
            SystemOp.Register(new CrossSceneDataHolder());
            SystemOp.Register(new User());
            SystemOp.Register(new SaveSystem());
            SystemOp.Register(new Flow());
            SystemOp.Register(new CacheValidator());
        }

        private void LoadSubsystemScene()
        {
            var sceneToLoad =
                currentStudioState == StudioState.Editor
                    ? configData.EditorSceneName
                    : configData.RuntimeSceneName;
            SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            OnSubsystemLoaded?.Invoke();
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
        }

        private void DisposeCurrentSubSystem(Action onUnloadComplete)
        {
            var subSystem = SystemOp.Resolve<ISubsystem>();
            subSystem?.Dispose();
            var operation = SceneManager.UnloadSceneAsync(
                currentActiveScene,
                UnloadSceneOptions.UnloadAllEmbeddedSceneObjects
            );
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
            if (nextIndex == 0)
                nextIndex++;
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
            SystemOp.Unregister<User>();
            SystemOp.Unregister<SaveSystem>();
            SystemOp.Unregister<Flow>();
            SystemOp.Unregister<CacheValidator>();
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

        private void AutoloadSceneIfSetFromConfig()
        {
            if (configData.ServeFromCloud) return;
            "UserName".TryGetPrefString(out var username);
            if (string.IsNullOrEmpty(username)) return;
            var projectName = configData.SceneDataToLoad.name;
            SystemOp.Resolve<User>().
                UpdateUserName(username).
                UpdateProjectName(projectName).
                UpdateProjectId(projectName);
            LoadSceneData();
        }

        public void SetSimulationState(bool isEnabled)
        {
            IsSimulating = isEnabled;
        }

        public void UpdateDefaultStudioState(StudioState state)
        {
            defaultStudioState = currentStudioState = state;
        }

        public void LoadSceneData()
        {
            if (defaultStudioState == StudioState.Editor)
            {
                if (configData.PickupSavedData)
                {
                    if (configData.ServeFromCloud)
                    {
                        SystemOp.Resolve<Flow>().DoCloudSaveCheck(LoadSubsystemScene);
                    }
                    else
                    {
                        SystemOp.Resolve<Flow>().DoLocalSaveCheck(LoadSubsystemScene);
                    }
                }
                else
                {
                    LoadSubsystemScene();
                }
            }
            else
            {
                currentStudioState = StudioState.Runtime;
                SystemOp.Resolve<Flow>().GetAndSetProjectDetailsForRuntime(LoadSubsystemScene);
            }
        }

        public void SwitchState()
        {
            DisposeCurrentSubSystem(LoadSubsystemScene);
            currentStudioState = GetNextState();
            previousStudioState = GetOtherState();
        }
    }
}
