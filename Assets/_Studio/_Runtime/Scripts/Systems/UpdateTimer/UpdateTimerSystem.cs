namespace Terra.Studio
{
    public class UpdateTimerSystem : BaseSystem<UpdateTimerComponent>
    {
        protected override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<UpdateTimerComponent>(entity);
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<InGameTimeHandler>();
            RuntimeOp.Resolve<InGameTimeHandler>().AddTime(entityRef.updateBy);
            Broadcast(entityRef);
        }
    }
}