using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class RuntimeSystem : MonoBehaviour, ISubsystem, IMouseEvents
    {
        private EcsWorld ecsWorld;
        private IEcsSystems updateSystems;
        private Dictionary<Type, object> typeToInstances;

        public EcsWorld World { get { return ecsWorld; } }
        public Action<GameObject> OnClicked { get; set; }

        private void Awake()
        {
            Interop<SystemInterop>.Current.Register(this as ISubsystem);
            Interop<RuntimeInterop>.Current.Register(this);
            Interop<RuntimeInterop>.Current.Register(new Broadcaster());
            Interop<RuntimeInterop>.Current.Register(new ComponentsData());
        }

        public void Initialize()
        {
            InitializeEcs();
        }

        private void InitializeEcs()
        {
            ecsWorld = new EcsWorld();
            InitializeUpdateSystems();
            Author<WorldAuthor>.Current.Generate();
        }

        private void InitializeUpdateSystems()
        {
            typeToInstances = new();
            typeToInstances
                .Add(new OscillateSystem()) //This system need not be running always
                .Add(new ClickSystem());
            updateSystems = new EcsSystems(ecsWorld);
            foreach (var item in typeToInstances)
            {
                updateSystems.Add(item.Value as IEcsRunSystem);
            }
            updateSystems.Init();
        }

        private void Update()
        {
            updateSystems?.Run();
        }

        public IAbsRunsystem AddRunningInstance<T>() where T : IAbsRunsystem
        {
            if (typeToInstances.ContainsKey(typeof(T)))
            {
                return (IAbsRunsystem)typeToInstances[typeof(T)];
            }
            var instance = Activator.CreateInstance<T>();
            typeToInstances.Add(typeof(T), instance);
            return (IAbsRunsystem)instance;
        }

        public IAbsRunsystem GetRunningInstance<T>()
        {
            if (typeToInstances.ContainsKey(typeof(T)))
            {
                return (T)typeToInstances[typeof(T)] as IAbsRunsystem;
            }
            return default;
        }

        public void RemoveRunningInstance<T>() where T : IAbsRunsystem
        {
            if (!typeToInstances.ContainsKey(typeof(T)))
            {
                return;
            }
            typeToInstances.Remove(typeof(T));
        }

        public void Dispose()
        {
            Author<WorldAuthor>.Flush();
            Author<ComponentAuthor>.Flush();
            Author<EntityAuthor>.Flush();
        }

        private void OnDestroy()
        {
            updateSystems?.Destroy();
            ecsWorld?.Destroy();
            Interop<RuntimeInterop>.Current.Unregister<Broadcaster>();
            Interop<RuntimeInterop>.Current.Unregister<ComponentsData>();
            Interop<SystemInterop>.Current.Unregister(this as ISubsystem);
            Interop<RuntimeInterop>.Current.Unregister(this);
        }
    }
}
