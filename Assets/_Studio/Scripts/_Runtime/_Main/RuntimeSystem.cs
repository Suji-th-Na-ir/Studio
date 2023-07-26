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
            Interop<RuntimeInterop>.Current.Register(new ConditionHolder());
            Interop<RuntimeInterop>.Current.Register(new BroadcastSystem());
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
                .Add(new OscillateSystem())
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

        public IAbsRunsystem GetRunningInstance<T>()
        {
            if (typeToInstances.ContainsKey(typeof(T)))
            {
                return (T)typeToInstances[typeof(T)] as IAbsRunsystem;
            }
            return default;
        }

        public void Dispose()
        {
            Author<WorldAuthor>.Flush();
            Author<ComponentAuthor>.Flush();
            Author<EntityAuthor>.Flush();
        }

        private void OnDestroy()
        {
            Interop<RuntimeInterop>.Current.Unregister<ConditionHolder>();
            Interop<RuntimeInterop>.Current.Unregister<BroadcastSystem>();
            Interop<SystemInterop>.Current.Unregister(this as ISubsystem);
            Interop<RuntimeInterop>.Current.Unregister(this);
        }
    }
}
