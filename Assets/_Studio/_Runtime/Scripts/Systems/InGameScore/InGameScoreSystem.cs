namespace Terra.Studio
{
    public class InGameScoreSystem : BaseSystem
    {
        public override void Init<T>(int entity)
        {
            var compData = EntityAuthorOp.GetComponent<InGameScoreComponent>(entity);
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<ScoreHandler>();
            RuntimeOp.Resolve<ScoreHandler>().targetScore = compData.targetScore;
            RuntimeOp.Resolve<ScoreHandler>().OnTargetScoreReached += () =>
            {
                if (compData.IsBroadcastable)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(compData.Broadcast);
                }
            };
        }
    }
}
