using System;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class PseudoRuntime<T> : IDisposable where T : BaseBehaviour
    {
        private GameObject originalTarget;
        private ISubsystem cachedsubsystem;

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
            EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(true);
            EditorOp.Resolve<SelectionHandler>().ToggleGizmo(false);
            SystemOp.Unregister(cachedsubsystem);
            LoadRuntime();
        }

        private void LoadRuntime()
        {
            cachedsubsystem = SystemOp.Resolve<ISubsystem>();
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
            var components = new EntityBasedComponent[1];
            var cherryPickedComponent = virtualEntity.components.Where(x => x.type.Equals(covariance)).First();
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
            originalTarget.SetActive(false);
        }

        private void OnRuntimeActive(Scene scene)
        {
            SystemOp.Resolve<ISubsystem>().Initialize(scene);
            CoroutineService.RunCoroutine(InitiateConditionalEvents, CoroutineService.DelayType.WaitForFrame);
        }

        private void InitiateConditionalEvents()
        {
            RuntimeOp.Resolve<ComponentsData>().ExecuteAllInterceptedEvents();
        }

        public void Dispose()
        {
            SystemOp.Resolve<ISubsystem>().Dispose();
            var unloadOperation = SceneManager.UnloadSceneAsync(RuntimeSceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            unloadOperation.completed += OnUnloadDone;
        }

        private void OnUnloadDone(AsyncOperation unloadOperation)
        {
            unloadOperation.completed -= OnUnloadDone;
            SystemOp.Resolve<System>().SetSimulationState(false);
            SystemOp.Resolve<System>().CanInitiateSubsystemProcess = () => { return true; };
            EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(false);
            EditorOp.Resolve<SelectionHandler>().ToggleGizmo(true);
            SystemOp.Register(cachedsubsystem);
            originalTarget.SetActive(true);
        }
    }
}