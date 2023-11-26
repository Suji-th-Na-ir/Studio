namespace Terra.Studio
{
    public class UpdateScoreSystem : BaseSystem<UpdateScoreComponent>
    {
        public override void Init(int entity)
        {
            base.Init(entity);
            ref var entityRef = ref entity.GetComponent<UpdateScoreComponent>();
            if (entityRef.AddScoreValue > 0)
            {
                RuntimeOp.Resolve<CoreGameManager>().EnableModule<ScoreHandler>();
            }
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<UpdateScoreComponent>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in UpdateScoreComponent component)
        {
            if (component.AddScoreValue != 0)
            {
                RuntimeOp.Resolve<ScoreHandler>().AddScore(component.AddScoreValue);
            }
            Broadcast(component);
        }
    }
}
