namespace Terra.Studio
{
    public class ResetTimerSystem : BaseSystem<ResetTimerComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref EntityAuthorOp.GetComponent<ResetTimerComponent>(entity);
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<InGameTimeHandler>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in ResetTimerComponent component)
        {
            RuntimeOp.Resolve<InGameTimeHandler>().ResetTime();
            Broadcast(component);
        }
    }
}