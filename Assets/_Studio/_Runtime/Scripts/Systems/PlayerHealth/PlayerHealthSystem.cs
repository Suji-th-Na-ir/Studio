namespace Terra.Studio
{
    public class PlayerHealthSystem : BaseSystem
    {
        public override void Init<T>(int entity)
        {
            var entityRef = entity.GetComponent<PlayerHealthComponent>();
            var playerData = RuntimeOp.Resolve<PlayerData>();
            if (entityRef.regenerateHealth)
            {
                playerData.UpdatePlayerRegenerationTime(entityRef.regenerationPerSec);
            }
            if (entityRef.IsBroadcastable)
            {
                playerData.UpdateBroadcastValueOnPlayerDied(entityRef.Broadcast);
            }
            RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
        }
    }
}