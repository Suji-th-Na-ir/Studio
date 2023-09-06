using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class CollideSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<CollideComponent>(entity);
            if (entityRef.listen != Listen.Always)
            {
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, entityRef.EventContext);
                entityRef.IsExecuted = true;
            }
            OnDemandRun(in entityRef, entity);
        }

        public void OnDemandRun(in CollideComponent component, int entity)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.RefObj.transform.position);
            }
            var listenMultipleTimes = component.listen == Listen.Always;
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, !listenMultipleTimes);
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<CollideComponent>().End();
            var compPool = currentWorld.GetPool<CollideComponent>();
            foreach (var entity in filter)
            {
                var component = compPool.Get(entity);
                if (component.IsExecuted)
                {
                    continue;
                }
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, component.EventContext);
            }
        }
    }
}
