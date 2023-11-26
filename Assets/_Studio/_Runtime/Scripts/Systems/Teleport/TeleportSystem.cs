namespace Terra.Studio
{
    public class TeleportSystem : BaseSystem<TeleportComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<TeleportComponent>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in TeleportComponent component)
        {
            RuntimeOp.Resolve<GameData>().SetPlayerPosition(component.teleportTo);
            PlayFXIfExists(component, 0);
            Broadcast(component);
        }
    }
}
