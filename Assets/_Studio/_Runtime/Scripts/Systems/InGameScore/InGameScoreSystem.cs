namespace Terra.Studio
{
    public class InGameScoreSystem : BaseSystem<InGameScoreComponent>
    {
        public override void Init(int entity)
        {
            var compData = EntityAuthorOp.GetComponent<InGameScoreComponent>(entity);
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<ScoreHandler>();
            RuntimeOp.Resolve<ScoreHandler>().targetScore = compData.targetScore;
            RuntimeOp.Resolve<ScoreHandler>().OnTargetScoreReached += () =>
            {
                Broadcast(compData);
            };
        }
    }
}