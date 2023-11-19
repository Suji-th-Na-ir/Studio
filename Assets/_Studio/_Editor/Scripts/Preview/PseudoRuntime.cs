using System;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class PseudoRuntime<T> : IDisposable where T : BaseBehaviour
    {
        private static readonly Vector3 FIXED_INFINITE_POSITION = new(-1000f, -1000f, -1000f);

        public event Action OnRuntimeInitialized;
        public event Action OnEventsExecuted;
        public event Action OnBroadcastExecuted;
        public Action ForceInitiateConditionalEvent => OnRuntimeDataInitialized;

        private GameObject originalTarget;
        private Scene cachedRuntimeScene;
        private Vector3 cachedPosition;

        private static string runtimeSceneName;
        private string RuntimeSceneName
        {
            get
            {
                if (string.IsNullOrEmpty(runtimeSceneName))
                {
                    runtimeSceneName = ((SystemConfigurationSO)SystemOp.Load(ResourceTag.SystemConfig)).RuntimeSceneName;
                }
                return runtimeSceneName;
            }
        }

        public static void GenerateFor(T behaviour, bool generate)
        {
            var aliveInstance = SystemOp.Resolve<PseudoRuntime<T>>();
            if (aliveInstance != null)
            {
                SystemOp.Unregister(aliveInstance);
            }
            if (generate)
            {
                var newInstance = new PseudoRuntime<T>(behaviour);
                SystemOp.Register(newInstance);
            }
        }

        public PseudoRuntime(T baseBehaviour)
        {
            DoExport(baseBehaviour);
            SystemOp.Resolve<System>().SetSimulationState(true);
            SystemOp.Resolve<System>().CanInitiateSubsystemProcess = () => { return false; };
            SystemOp.Unregister(EditorOp.Resolve<EditorSystem>() as ISubsystem);
            LoadRuntime();
        }

        private void LoadRuntime()
        {
            var operation = SceneManager.LoadSceneAsync(RuntimeSceneName, LoadSceneMode.Additive);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode __)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            CoroutineService.RunCoroutine(() => { OnRuntimeActive(scene); }, CoroutineService.DelayType.WaitUntil, predicate: IsRuntimeActive);
        }

        private bool IsRuntimeActive()
        {
            var result = RuntimeOp.Resolve<RuntimeSystem>();
            if (result)
            {
                return true;
            }
            return false;
        }

        private void DoExport(T baseBehaviour)
        {
            var covariance = EditorOp.Resolve<DataProvider>().GetCovariance(baseBehaviour);
            var virtualEntity = EditorOp.Resolve<SceneDataHandler>().GetVirtualEntity(baseBehaviour.gameObject, 0, true);
            var cherryPickedComponent = virtualEntity.components.Where(x => x.type.Equals(covariance)).First();
            virtualEntity.components = new EntityBasedComponent[] { cherryPickedComponent };
            if (virtualEntity.children != null)
            {
                for (int i = 0; i < virtualEntity.children.Length; i++)
                {
                    virtualEntity.children[i].components = new EntityBasedComponent[0];
                }
            }
            var worldData = new WorldData()
            {
                entities = new VirtualEntity[] { virtualEntity },
                metaData = new WorldMetaData()
            };
            var json = JsonConvert.SerializeObject(worldData);
            SystemOp.Resolve<CrossSceneDataHolder>().Set(json);
            originalTarget = baseBehaviour.gameObject;
            SetObjectPathData();
            cachedPosition = originalTarget.transform.position;
            originalTarget.transform.position = FIXED_INFINITE_POSITION;
        }

        private void SetObjectPathData()
        {
            if (originalTarget.TryGetComponent(out StudioGameObject studioGameObject))
            {
                var assetPath = studioGameObject.itemData.ResourcePath;
                SystemOp.Resolve<CrossSceneDataHolder>().Set(assetPath, originalTarget);
            }
        }

        private void OnRuntimeActive(Scene scene)
        {
            cachedRuntimeScene = scene;
            SystemOp.Resolve<ISubsystem>().Initialize(scene);
            RuntimeOp.Resolve<Broadcaster>().OnToBroadcastRequestReceived = OnBroadcastExecuted;
            OnRuntimeDataInitialized();
        }

        private void OnRuntimeDataInitialized()
        {
            OnRuntimeInitialized?.Invoke();
            CoroutineService.RunCoroutine(InitiateConditionalEvents, CoroutineService.DelayType.WaitForXSeconds, BehaviourPreview.CUSTOM_TIME_DELATION);
        }

        private void InitiateConditionalEvents()
        {
            OnEventsExecuted?.Invoke();
            RuntimeOp.Resolve<ComponentsData>().ExecuteAllInterceptedEvents();
        }

        public void Dispose()
        {
            RuntimeOp.Resolve<Broadcaster>().OnToBroadcastRequestReceived = null;
            SystemOp.Resolve<ISubsystem>().Dispose();
            var unloadOperation = SceneManager.UnloadSceneAsync(RuntimeSceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            unloadOperation.completed += OnUnloadDone;
        }

        private void OnUnloadDone(AsyncOperation unloadOperation)
        {
            unloadOperation.completed -= OnUnloadDone;
            SystemOp.Resolve<System>().SetSimulationState(false);
            SystemOp.Resolve<System>().CanInitiateSubsystemProcess = () => { return true; };
            SystemOp.Register(EditorOp.Resolve<EditorSystem>() as ISubsystem);
            originalTarget.transform.position = cachedPosition;
        }

        public void OnRestartRequested()
        {
            SetObjectPathData();
            var system = RuntimeOp.Resolve<RuntimeSystem>();
            RuntimeOp.Resolve<EntitiesGraphics>()?.FlushAllTrackedVisuals();
            system.Dispose();
            system.UnstageAll();
            OnRuntimeActive(cachedRuntimeScene);
        }
    }
}