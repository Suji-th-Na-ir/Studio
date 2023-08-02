using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.Rotate")]
    public class RotateAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var tuple = ((int id, string type, string compData, GameObject obj))data;
            var compData = JsonConvert.DeserializeObject<RotateComponent>(tuple.compData);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<RotateComponent>();
            compPool.Add(tuple.id);
            ref var compRef = ref compPool.Get(tuple.id);
            Helper.CopyStructFieldValues(compData, ref compRef);
            compRef.IsConditionAvailable = compData.IsConditionAvailable;
            compRef.ConditionType = compData.ConditionType;
            compRef.ConditionData = compData.ConditionData;
            compRef.IsBroadcastable = compData.IsBroadcastable;
            compRef.Broadcast = compData.Broadcast;
            compRef.IsTargeted = compData.IsTargeted;
            compRef.TargetId = compData.TargetId;
            compRef.refObj = tuple.obj;
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<RotateSystem>();
            instance.Init(ecsWorld, tuple.id);
        }
    }
}