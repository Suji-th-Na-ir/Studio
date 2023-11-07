using System;
using UnityEngine;
using System.Linq;
using Leopotam.EcsLite;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class RuntimeSystem : MonoBehaviour, ISubsystem, IMouseEvents
    {
        private bool canRunSystems;
        private EcsWorld ecsWorld;
        private IEcsSystems globalUpdateSystems;
        private Dictionary<Type, object> typeToInstances;
        private Dictionary<Type, IEcsSystems> customUpdateSystems;
        private Dictionary<Type, IEcsSystems>.Enumerator? customUpdateEnumerator;
        private List<Type> customUpdateSystemToRemove;
        private List<BaseCoroutineRunner> coroutineRunners;
        private Scene scene;
        private Action onPreInitialisationCompleted;

        public EcsWorld World { get { return ecsWorld; } }
        public Action<GameObject> OnClicked { get; set; }

        private void Awake()
        {
            SystemOp.Register(this as ISubsystem);
            RuntimeOp.Register(this);
        }

        public void Initialize(Scene scene)
        {
            this.scene = scene;
            ResolveEssentials();
            RuntimeOp.Resolve<GameStateHandler>().SubscribeToGameStart(true, (data) => { canRunSystems = true; });
            RuntimeOp.Resolve<GameStateHandler>().SubscribeToGameEnd(true, (data) => { DestroyEcsSystemsAndWorld(); });
            InitializeEcs();
        }

        private void ResolveEssentials()
        {
            RuntimeOp.Register(new Broadcaster());
            RuntimeOp.Register(new ComponentsData());
            RuntimeOp.Register(new CoreGameManager());
            RuntimeOp.Register(new SceneDataHandler());
            RuntimeOp.Register(new EntitiesGraphics());
        }

        private void InitializeEcs()
        {
            ecsWorld = new EcsWorld();
            InitializeUpdateSystems();
            onPreInitialisationCompleted = InitializeStateBasedOnSystemCondition;
            WorldAuthorOp.Generate(onPreInitialisationCompleted);
        }

        private void InitializeStateBasedOnSystemCondition()
        {
            var canSwitch = SystemOp.Resolve<System>().CanInitiateSubsystemProcess?.Invoke() ?? true;
            if (canSwitch)
            {
                RuntimeOp.Resolve<GameStateHandler>().SwitchToNextState();
            }
            else
            {
                canRunSystems = true;
            }
        }

        private void InitializeUpdateSystems()
        {
            customUpdateSystems = new();
            typeToInstances = new();
            customUpdateSystemToRemove = new();
            coroutineRunners = new();
            globalUpdateSystems = new EcsSystems(ecsWorld)
                                    .Add(new InputSystem());
            globalUpdateSystems.Init();
        }

        private void Update()
        {
            if (canRunSystems)
            {
                globalUpdateSystems?.Run();
                UpdateCustomSystems();
            }
        }

        private void UpdateCustomSystems()
        {
            if (customUpdateEnumerator == null)
            {
                return;
            }
            var enumerator = customUpdateEnumerator.Value;
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value?.Run();
            }
            var isModified = false;
            foreach (var toRemoveType in customUpdateSystemToRemove)
            {
                isModified = true;
                customUpdateSystems.Remove(toRemoveType);
            }
            if (isModified)
            {
                customUpdateSystemToRemove.Clear();
                customUpdateEnumerator = customUpdateSystems.GetEnumerator();
            }
        }

        public BaseSystem AddRunningInstance<T>() where T : BaseSystem
        {
            var type = typeof(T);
            if (typeToInstances.ContainsKey(type))
            {
                return (BaseSystem)typeToInstances[type];
            }
            var instance = Activator.CreateInstance<T>();
            typeToInstances.Add(type, instance);
            if (type.GetInterfaces().Contains(typeof(IEcsRunSystem)))
            {
                var newSystem = new EcsSystems(World).Add((IEcsRunSystem)instance);
                newSystem.Init();
                customUpdateSystems.Add(type, newSystem);
                customUpdateEnumerator = customUpdateSystems.GetEnumerator();
            }
            return instance;
        }

        public void RemoveRunningInstance<T>(T _) where T : BaseSystem
        {
            var type = typeof(T);
            if (!typeToInstances.ContainsKey(type))
            {
                return;
            }
            typeToInstances.Remove(type);
            if (type.GetInterfaces().Contains(typeof(IEcsRunSystem)))
            {
                if (customUpdateSystems.ContainsKey(type))
                {
                    var system = customUpdateSystems[type];
                    customUpdateSystemToRemove.Add(type);
                    system?.Destroy();
                }
            }
        }

        public void AddCoroutineRunner(bool add, BaseCoroutineRunner runner)
        {
            if (add)
            {
                coroutineRunners?.Add(runner);
            }
            else
            {
                coroutineRunners?.Remove(runner);
            }
        }

        public void RequestSwitchState()
        {
            SystemOp.Resolve<System>().SwitchState();
        }

        public void Dispose()
        {
            DestroyEcsSystemsAndWorld();
            RuntimeOp.Resolve<EntitiesGraphics>().FlushAllTrackedVisuals();
            WorldAuthorOp.Flush();
            ComponentAuthorOp.Flush();
            EntityAuthorOp.Flush();
            RuntimeOp.Unregister<CoreGameManager>();
            RuntimeOp.Unregister<SceneDataHandler>();
            RuntimeOp.Unregister<EntitiesGraphics>();
        }

        private void DestroyEcsSystemsAndWorld()
        {
            canRunSystems = false;
            globalUpdateSystems?.Destroy();
            if (customUpdateSystems != null && customUpdateSystems.Count != 0)
            {
                foreach (var customSystem in customUpdateSystems)
                {
                    customSystem.Value?.Destroy();
                }
                customUpdateSystems = null;
            }
            if (typeToInstances != null && typeToInstances.Count != 0)
            {
                foreach (var type in typeToInstances)
                {
                    var instance = (BaseSystem)type.Value;
                    instance.OnHaltRequested(ecsWorld);
                }
                typeToInstances = null;
            }
            if (coroutineRunners != null && coroutineRunners.Count != 0)
            {
                for (int i = 0; i < coroutineRunners.ToArray().Length; i++)
                {
                    Destroy(coroutineRunners[i]);
                }
                coroutineRunners = null;
            }
            ecsWorld?.Destroy();
        }

        public Scene GetScene()
        {
            return scene;
        }

        public void UnstageAll()
        {
            RuntimeOp.Unregister<Broadcaster>();
            RuntimeOp.Unregister<ComponentsData>();
        }

        private void OnDestroy()
        {
            UnstageAll();
            SystemOp.Unregister(this as ISubsystem);
            RuntimeOp.Unregister(this);
        }
    }
}
