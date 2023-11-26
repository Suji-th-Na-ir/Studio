using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class RespawnSystem : BaseSystem<RespawnComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<RespawnComponent>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in RespawnComponent component)
        {
            PlayFXIfExists(component, 0);
            Broadcast(component);
            var respawnPoint = RuntimeOp.Resolve<GameData>().RespawnPoint;
            RuntimeOp.Resolve<PlayerData>().SetPlayerPosition(respawnPoint);
        }
    }
}
