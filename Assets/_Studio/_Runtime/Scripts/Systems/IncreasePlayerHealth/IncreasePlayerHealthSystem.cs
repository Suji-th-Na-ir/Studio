namespace Terra.Studio
{
    public class IncreasePlayerHealthSystem : BaseSystem<IncreasePlayerHealthComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<IncreasePlayerHealthComponent>(entity);
            RuntimeOp.Resolve<PlayerData>().OnPlayerHealthChangeRequested?.Invoke((int)entityRef.playerHealthModifier);
            PlayFXIfExists(entityRef, 0);
            Broadcast(entityRef);
        }
    }
}