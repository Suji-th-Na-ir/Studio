using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class CheckpointSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            ref var entityRef = ref entity.GetComponent<CheckpointComponent>();
            entityRef.IsExecuted = true;
            compsData.ProvideEventContext(false, entityRef.EventContext);
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in CheckpointComponent component)
        {
            RuntimeOp.Resolve<GameData>().RespawnPoint = component.respawnPoint;
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
            var filter = currentWorld.Filter<CheckpointComponent>().End();
            var checkPointPool = currentWorld.GetPool<CheckpointComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                var checkpoint = checkPointPool.Get(entity);
                if (checkpoint.IsExecuted)
                {
                    continue;
                }
                compsData.ProvideEventContext(false, checkpoint.EventContext);
            }
        }
    }
}
