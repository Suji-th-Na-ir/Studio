namespace Terra.Studio
{
    public class DestroyOnSystem : BaseSystem<DestroyOnComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref EntityAuthorOp.GetComponent<DestroyOnComponent>(entity);
            OnDemandRun(in entityRef, entity);
        }

        public void OnDemandRun(in DestroyOnComponent component, int entityID)
        {
            PlayFXIfExists(component, 0);
            Broadcast(component);
            DeleteEntity(entityID);
        }
    }
}
