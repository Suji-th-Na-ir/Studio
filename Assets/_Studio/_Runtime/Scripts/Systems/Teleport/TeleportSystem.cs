using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class TeleportSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<TeleportComponent>();
            if (entityRef.listen != Listen.Always)
            {
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, entityRef.EventContext);
                entityRef.IsExecuted = true;
            }
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in TeleportComponent component)
        {
            RuntimeOp.Resolve<PlayerData>().SetPlayerPosition(component.teleportTo);
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
            var filter = currentWorld.Filter<TeleportComponent>().End();
            var compPool = currentWorld.GetPool<TeleportComponent>();
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
