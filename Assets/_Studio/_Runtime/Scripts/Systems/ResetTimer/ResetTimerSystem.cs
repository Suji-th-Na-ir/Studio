using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class ResetTimerSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<ResetTimerComponent>(entity);
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<InGameTimeHandler>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in ResetTimerComponent component)
        {
            RuntimeOp.Resolve<InGameTimeHandler>().ResetTime();
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
            var filter = currentWorld.Filter<ResetTimerComponent>().End();
            var compPool = currentWorld.GetPool<ResetTimerComponent>();
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
