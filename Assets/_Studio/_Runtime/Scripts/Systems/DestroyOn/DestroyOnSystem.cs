using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class DestroyOnSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<DestroyOnComponent>(entity);
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(false, entityRef.EventContext);
            entityRef.IsExecuted = true;
            OnDemandRun(in entityRef, entity);
        }

        public void OnDemandRun(in DestroyOnComponent component, int entityID)
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
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast);
            }
            EntityAuthorOp.Degenerate(entityID);
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<DestroyOnComponent>().End();
            var destroyPool = currentWorld.GetPool<DestroyOnComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                var destroyable = destroyPool.Get(entity);
                if (destroyable.IsExecuted)
                {
                    continue;
                }
                compsData.ProvideEventContext(false, destroyable.EventContext);
            }
        }
    }
}
