using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class UpdateTimerSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<UpdateTimerComponent>(entity);
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<InGameTimeHandler>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in UpdateTimerComponent component)
        {
            RuntimeOp.Resolve<InGameTimeHandler>().AddTime(component.updateBy);
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
            var filter = currentWorld.Filter<UpdateTimerComponent>().End();
            var compPool = currentWorld.GetPool<UpdateTimerComponent>();
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
