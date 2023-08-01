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

        public EcsWorld World { get { return ecsWorld; } }
        public Action<GameObject> OnClicked { get; set; }

        private void Awake()
        {
            SystemOp.Register(this as ISubsystem);
            RuntimeOp.Register(this);
        }

        public void Initialize()
        {
            RuntimeOp.Register(new Broadcaster());
            RuntimeOp.Register(new ComponentsData());
            InitializeEcs();
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
            try
            {
                if (customUpdateEnumerator != null)
                {
                    var enumerator = customUpdateEnumerator.Value;
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Value?.Run();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Gotten error at custom update");
                Debug.LogError(ex);
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
            return (IAbsRunsystem)instance;
        }

        public void RemoveRunningInstance<T>(T instance) where T : IAbsRunsystem
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
                    customUpdateSystems.Remove(type);
                    system?.Destroy();
                    customUpdateEnumerator = customUpdateSystems.GetEnumerator();
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
            SystemOp.Unregister(this as ISubsystem);
            RuntimeOp.Unregister(this);
        }
    }
}
