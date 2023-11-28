using Leopotam.EcsLite;

namespace Terra.Studio
{
    public partial class BaseSystem<T> : IWorldActions where T : struct, IBaseComponent
    {
        public virtual void Init(int entity)
        {
            ref var entityRef = ref entity.GetComponent<T>();
            var eventContext = entityRef.EventContext;
            eventContext.onConditionMet = (obj) =>
            {
                OnConditionalCheck(entity, obj);
            };
            entityRef.EventContext = eventContext;
            entityRef.IsExecuted = false;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(true, entityRef.EventContext);
        }

        public virtual void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<T>().End();
            var pool = currentWorld.GetPool<T>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                var component = pool.Get(entity);
                if (component.EventContext.Equals(default(EventContext)) || component.IsExecuted) continue;
                compsData.ProvideEventContext(false, component.EventContext);
            }
        }

        protected virtual void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<T>();
            if (entityRef.Listen == Listen.Once)
            {
                entityRef.IsExecuted = true;
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, entityRef.EventContext);
            }
        }

        protected virtual void PlayFXIfExists(in T instance, int index)
        {
            PlaySFXIfExists(instance, index);
            PlayVFXIfExists(instance, index);
        }

        protected virtual void PlaySFXIfExists(in T instance, int index)
        {
            var baseComponent = (IBaseComponent)instance;
            var fxData = baseComponent.GetSFXData(index);
            if (fxData != null && fxData.Value.canPlay)
            {
                RuntimeWrappers.PlaySFX(fxData.Value.clipName);
            }
        }

        protected virtual void PlayVFXIfExists(in T instance, int index)
        {
            var baseComponent = (IBaseComponent)instance;
            var fxData = baseComponent.GetVFXData(index);
            if (fxData != null && fxData.Value.canPlay)
            {
                RuntimeWrappers.PlayVFX(fxData.Value.clipName, instance.RefObj.transform.position);
            }
        }

        protected virtual void Broadcast(in T instance)
        {
            if (instance.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(instance.Broadcast);
            }
        }

        protected virtual void RemoveRunningInstance()
        {
            RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance<BaseSystem<T>, T>();
        }

        protected virtual void DeleteEntity(int entity)
        {
            EntityAuthorOp.Degenerate<T>(entity);
        }

        public virtual void OnEntityQueuedToDestroy(int entity) { }
    }
}