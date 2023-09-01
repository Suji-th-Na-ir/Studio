using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.Checkpoint")]
    public class CheckpointAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var component = JsonConvert.DeserializeObject<CheckpointComponent>(authorData.compData);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<CheckpointComponent>();
            compPool.Add(authorData.entity);
            ref var compRef = ref compPool.Get(authorData.entity);
            component.CloneFrom(compRef);
            compRef.IsConditionAvailable = component.IsConditionAvailable;
            compRef.ConditionType = component.ConditionType;
            compRef.ConditionData = component.ConditionData;
            compRef.IsBroadcastable = component.IsBroadcastable;
            compRef.Broadcast = component.Broadcast;
            compRef.IsTargeted = component.IsTargeted;
            compRef.TargetId = component.TargetId;
            compRef.refObj = authorData.obj;
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<CheckpointSystem>();
            instance.Init(ecsWorld, authorData.entity);
        }
    }
}