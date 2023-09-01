using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class CheckpointSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            ref var entityRef = ref EntityAuthorOp.GetComponent<CheckpointComponent>(entity);
            entityRef.IsExecuted = true;
            compsData.ProvideEventContext(false, entityRef.EventContext);
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var checkpointPool = world.GetPool<CheckpointComponent>();
            OnDemandRun(in checkpointPool.Get(entity));
        }

        public void OnDemandRun(in CheckpointComponent component)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.RefObj.transform.position);
            }
            RuntimeOp.Resolve<GameData>().RespawnPoint = component.respawnPoint;
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
