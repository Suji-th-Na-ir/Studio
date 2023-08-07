using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.Respawn")]
    public class RespawnAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var tuple = ((int id, string type, string compData, GameObject obj))data;
            var compData = JsonConvert.DeserializeObject<RespawnComponent>(tuple.compData);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<RespawnComponent>();
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
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<RespawnSystem>();
            instance.Init(ecsWorld, tuple.id);
        }
    }
}