using System;
using UnityEngine;
using System.Linq;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class RuntimeSystem : MonoBehaviour, ISubsystem, IMouseEvents
    {
        private EcsWorld ecsWorld;
        private IEcsSystems globalUpdateSystems;
        private Dictionary<Type, object> typeToInstances;
        private Dictionary<Type, IEcsSystems> customUpdateSystems;
        private Dictionary<Type, IEcsSystems>.Enumerator? customUpdateEnumerator;
        private List<Type> customUpdateSystemToRemove;

        public EcsWorld World { get { return ecsWorld; } }
        public Action<GameObject> OnClicked { get; set; }

        private void Awake()
        {
            SystemOp.Register(this as ISubsystem);
            RuntimeOp.Register(this);
        }

        public void Initialize()
        {
            ResolveEssentials();
            InitializeEcs();
            RuntimeOp.Resolve<GameStateHandler>().SwitchToNextState();
        }

        private void ResolveEssentials()
        {
            RuntimeOp.Register(new Broadcaster());
            RuntimeOp.Register(new ComponentsData());
            RuntimeOp.Register(new GameStateHandler());
            RuntimeOp.Register(new GameData());
            RuntimeOp.Resolve<GameData>().PlayerRef = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void InitializeEcs()
        {
            ecsWorld = new EcsWorld();
            InitializeUpdateSystems();
            WorldAuthorOp.Generate();
        }

        private void InitializeUpdateSystems()
        {
            customUpdateSystems = new();
            typeToInstances = new();
            customUpdateSystemToRemove = new();
            globalUpdateSystems = new EcsSystems(ecsWorld)
                                    .Add(new ClickSystem());
            globalUpdateSystems.Init();
        }

        private void Update()
        {
            globalUpdateSystems?.Run();
            UpdateCustomSystems();
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

        public IAbsRunsystem AddRunningInstance<T>() where T : IAbsRunsystem
        {
            var type = typeof(T);
            if (typeToInstances.ContainsKey(type))
            {
                return (IAbsRunsystem)typeToInstances[type];
            }
            var instance = Activator.CreateInstance<T>();
            typeToInstances.Add(type, instance);
            if (typeof(T).GetInterfaces().Contains(typeof(IEcsRunSystem)))
            {
                var newSystem = new EcsSystems(World).Add((IEcsRunSystem)instance);
                newSystem.Init();
                customUpdateSystems.Add(type, newSystem);
                customUpdateEnumerator = customUpdateSystems.GetEnumerator();
            }
            return instance;
        }

        public void RemoveRunningInstance<T>(T _) where T : IAbsRunsystem
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

        public void RequestSwitchState()
        {
            //Check for busy state of the system, if there is any switch state already in progress
            SystemOp.Resolve<System>().SwitchState();
        }

        public void Dispose()
        {
            WorldAuthorOp.Flush();
            ComponentAuthorOp.Flush();
            EntityAuthorOp.Flush();
        }

        private void DestroyAllUpdatableSystems()
        {
            if (customUpdateSystems == null) return;
            foreach (var customSystem in customUpdateSystems)
            {
                customSystem.Value?.Destroy();
            }
            customUpdateSystems = null;
        }

        private void OnDestroy()
        {
            DestroyAllUpdatableSystems();
            globalUpdateSystems?.Destroy();
            ecsWorld?.Destroy();
            RuntimeOp.Unregister<Broadcaster>();
            RuntimeOp.Unregister<ComponentsData>();
            RuntimeOp.Unregister<GameStateHandler>();
            RuntimeOp.Unregister<GameData>();
            SystemOp.Unregister(this as ISubsystem);
            RuntimeOp.Unregister(this);
        }
    }
}
