using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.Checkpoint")]
    public class CheckpointAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var (id, _, compData, obj) = ((int, string, string, GameObject))data;
            var component = JsonConvert.DeserializeObject<CheckpointComponent>(compData);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<CheckpointComponent>();
            compPool.Add(id);
            ref var compRef = ref compPool.Get(id);
            Helper.CopyStructFieldValues(component, ref compRef);
            compRef.IsConditionAvailable = component.IsConditionAvailable;
            compRef.ConditionType = component.ConditionType;
            compRef.ConditionData = component.ConditionData;
            compRef.IsBroadcastable = component.IsBroadcastable;
            compRef.Broadcast = component.Broadcast;
            compRef.IsTargeted = component.IsTargeted;
            compRef.TargetId = component.TargetId;
            compRef.refObj = obj;
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<CheckpointSystem>();
            instance.Init(ecsWorld, id);
        }
    }
}