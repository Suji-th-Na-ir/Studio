using System;
using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class RuntimeSystem : MonoBehaviour, ISubsystem, IMouseEvents
    {
        private EcsWorld ecsWorld;
        private IEcsSystems initSystems;
        private IEcsSystems updateSystems;
        private IEcsSystems fixedUpdateSystems;

        public EcsWorld World { get { return ecsWorld; } }
        public Action<GameObject> OnClicked { get; set; }

        private bool systemsAdded = false;
            
        private void Awake()
        {
            Interop<SystemInterop>.Current.Register(this as ISubsystem);
            Interop<RuntimeInterop>.Current.Register(this);
        }

        private void Start()
        {
            systemsAdded = false;
            ecsWorld = new EcsWorld();
            initSystems = new EcsSystems(ecsWorld);
            updateSystems = new EcsSystems(ecsWorld)
                .Add(new ClickSystem());
            // updateSystems.Init();
            fixedUpdateSystems = new EcsSystems(ecsWorld);
            Author<WorldAuthor>.Current.Generate();
        }

        private void Update()
        {
            if(systemsAdded)
                updateSystems.Run();
        }

        public void AddUpdateSystem<T>(T runSystem) where T : IEcsRunSystem
        {
            if (updateSystems.GetAllSystems().Contains(runSystem))
            {
                return;
            }
            updateSystems.Add(runSystem);
            updateSystems.Init();
            systemsAdded = true;
        }

        public void RemoveUpdateSystem<T>(T runSystem) where T : IEcsRunSystem
        {
            if (!updateSystems.GetAllSystems().Contains(runSystem))
            {
                return;
            }
            //Figure out how to remove the added entities
        }

        public void Dispose()
        {
            Author<WorldAuthor>.Flush();
            Author<ComponentAuthor>.Flush();
            Author<EntityAuthor>.Flush();
        }

        private void OnDestroy()
        {
            Interop<SystemInterop>.Current.Unregister(this as ISubsystem);
            Interop<RuntimeInterop>.Current.Unregister(this);
        }
    }
}
