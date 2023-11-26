namespace Terra.Studio
{
    public class CheckpointSystem : BaseSystem<CheckpointComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<CheckpointComponent>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in CheckpointComponent component)
        {
            RuntimeOp.Resolve<GameData>().RespawnPoint = component.respawnPoint;
            PlayFXIfExists(component, 0);
            Broadcast(component);
        }
    }
}