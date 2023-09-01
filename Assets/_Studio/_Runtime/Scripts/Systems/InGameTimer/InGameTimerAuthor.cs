using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.InGameTimer")]
    public class InGameTimerAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<InGameTimerComponent>(authorData.compData);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<InGameTimerComponent>();
            compPool.Add(authorData.entity);
            ref var compRef = ref compPool.Get(authorData.entity);
            Helper.CopyStructFieldValues(compData, ref compRef);
            compRef.IsConditionAvailable = compData.IsConditionAvailable;
            compRef.ConditionType = compData.ConditionType;
            compRef.ConditionData = compData.ConditionData;
            compRef.IsBroadcastable = compData.IsBroadcastable;
            compRef.Broadcast = compData.Broadcast;
            compRef.IsTargeted = compData.IsTargeted;
            compRef.TargetId = compData.TargetId;
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<InGameTimerSystem>();
            instance.Init(ecsWorld, authorData.entity);
        }
    }
}