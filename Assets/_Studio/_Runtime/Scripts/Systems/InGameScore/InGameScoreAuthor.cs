using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.GameScore")]
    public class InGameScoreAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var tuple = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<InGameScoreComponent>(tuple.compData);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<InGameScoreComponent>();
            compPool.Add(tuple.entity);
            ref var compRef = ref compPool.Get(tuple.entity);
            Helper.CopyStructFieldValues(compData, ref compRef);
            compRef.IsBroadcastable = compData.IsBroadcastable;
            compRef.Broadcast = compData.Broadcast;
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<ScoreHandler>();
            RuntimeOp.Resolve<ScoreHandler>().targetScore = compRef.targetScore;
            RuntimeOp.Resolve<ScoreHandler>().OnTargetScoreReached += () =>
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(compData.Broadcast, true);
            };
        }
    }
}