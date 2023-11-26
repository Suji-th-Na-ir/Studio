namespace Terra.Studio
{
    public class SetObjectPositionSystem : BaseSystem<SetObjectPositionComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<SetObjectPositionComponent>();
            entityRef.CanExecute = true;
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in SetObjectPositionComponent component)
        {
            PlayFXIfExists(component, 0);
            Broadcast(component);
            component.RefObj.transform.position = component.targetPosition;
        }
    }
}