namespace Terra.Studio
{
    public class DecreasePlayerHealthSystem : BaseSystem<DecreasePlayerHealthComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref EntityAuthorOp.GetComponent<DecreasePlayerHealthComponent>(entity);
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in DecreasePlayerHealthComponent component)
        {
            var value = (int)component.playerHealthModifier * -1;
            RuntimeOp.Resolve<PlayerData>().OnPlayerHealthChangeRequested?.Invoke(value);
            PlayFXIfExists(component, 0);
            Broadcast(component);
        }
    }
}
