using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class PushSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<PushComponent>(entity);
        }

        public void OnDemandRun(in PushComponent component, int entity)
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
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<PushComponent>().End();
            var compPool = currentWorld.GetPool<PushComponent>();
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
