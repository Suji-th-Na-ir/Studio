using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class RespawnSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<RespawnComponent>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in RespawnComponent component)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlaySFX(component.vfxName);
            }
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
            var respawnPoint = RuntimeOp.Resolve<GameData>().RespawnPoint;
            RuntimeOp.Resolve<PlayerData>().SetPlayerPosition(respawnPoint);
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<RespawnComponent>().End();
            var respawnPool = currentWorld.GetPool<RespawnComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                var respawn = respawnPool.Get(entity);
                if (respawn.IsExecuted)
                {
                    continue;
                }
                compsData.ProvideEventContext(false, respawn.EventContext);
            }
        }
    }
}
