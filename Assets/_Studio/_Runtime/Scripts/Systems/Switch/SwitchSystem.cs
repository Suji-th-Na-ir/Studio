using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class SwitchSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<SwitchComponent>();
            if (entityRef.ConditionData.Equals("Terra.Studio.MouseAction"))
            {
                if (data == null)
                {
                    return;
                }
                var go = (GameObject)data;
                if (go != entityRef.RefObj)
                {
                    return;
                }
            }
            entityRef.UpdateState();
            OnDemandRun(in entityRef, entity);
        }

        public void OnDemandRun(in SwitchComponent component, int _)
        {
            var data = component.GetData();
            if (data.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(data.sfxName);
            }
            if (data.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(data.vfxName, component.RefObj.transform.position);
            }
            var listenMultipleTimes = component.listen == Listen.Always;
            if (data.isBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(data.broadcast, !listenMultipleTimes);
            }
            if (!listenMultipleTimes)
            {
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, component.EventContext);
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<SwitchComponent>().End();
            var compPool = currentWorld.GetPool<SwitchComponent>();
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
