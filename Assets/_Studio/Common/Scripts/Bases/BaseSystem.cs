using Leopotam.EcsLite;

namespace Terra.Studio
{
    public partial class BaseSystem
    {
        public virtual void Init<T>(int entity) where T : struct, IBaseComponent
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<T>(entity);
            var eventContext = entityRef.EventContext;
            eventContext.onConditionMet = (obj) =>
            {
                OnConditionalCheck(entity, obj);
            };
            entityRef.EventContext = eventContext;
            entityRef.IsExecuted = false;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(true, entityRef.EventContext);
            if (entityRef.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(entityRef.Broadcast);
            }
        }
        public virtual void OnConditionalCheck(int entity, object data) { }
        public virtual void OnHaltRequested(EcsWorld currentWorld) { }
    }
}
