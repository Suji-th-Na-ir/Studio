namespace Terra.Studio
{
    public class ClickSystem : BaseSystem<ClickComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<ClickComponent>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in ClickComponent component)
        {
            PlayFXIfExists(component, 0);
            Broadcast(component);
        }
    }
}