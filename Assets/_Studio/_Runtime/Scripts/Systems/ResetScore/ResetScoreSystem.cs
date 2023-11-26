namespace Terra.Studio
{
    public class ResetScoreSystem : BaseSystem<ResetScoreComponent>
    {
        public override void Init(int entity)
        {
            base.Init(entity);
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<ScoreHandler>();
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<ResetScoreComponent>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in ResetScoreComponent component)
        {
            RuntimeOp.Resolve<ScoreHandler>().ResetScore();
            Broadcast(component);
        }
    }
}