namespace Terra.Studio
{
    public class SwitchSystem : BaseSystem<SwitchComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<SwitchComponent>();
            entityRef.UpdateState();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in SwitchComponent component)
        {
            var data = component.GetData();
            var index = data.state == SwitchState.On ? 1 : 0;
            PlayFXIfExists(component, index);
            if (data.isBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(data.broadcast);
            }
        }
    }
}