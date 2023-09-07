using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class SetObjectPositionSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<SetObjectPositionComponent>(entity);
            entityRef.CanExecute = true;
            entityRef.IsExecuted = true;
            var comp = RuntimeOp.Resolve<ComponentsData>();
            comp.ProvideEventContext(false, entityRef.EventContext);
            OnDemandRun(in entityRef, entity);
        }

        public void OnDemandRun(in SetObjectPositionComponent component, int _)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.RefObj.transform.position);
            }
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
            component.RefObj.transform.position = component.targetPosition;
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<SetObjectPositionComponent>().End();
            var compPool = currentWorld.GetPool<SetObjectPositionComponent>();
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
