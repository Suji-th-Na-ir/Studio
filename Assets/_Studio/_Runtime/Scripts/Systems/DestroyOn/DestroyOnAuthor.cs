using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.DestroyOn")]
    public class DestroyOnAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var tuple = ((int id, string type, string compData, GameObject obj))data;
            var compData = JsonConvert.DeserializeObject<DestroyOnComponent>(tuple.compData);
            var runtimeSystem = RuntimeOp.Resolve<RuntimeSystem>();
            var compPool = runtimeSystem.World.GetPool<DestroyOnComponent>();
            compPool.Add(tuple.id);
            ref var compRef = ref compPool.Get(tuple.id);
            Helper.CopyStructFieldValues(compData, ref compRef);
            compRef.CanExecute = compData.CanExecute;
            compRef.IsConditionAvailable = compData.IsConditionAvailable;
            compRef.ConditionType = compData.ConditionType;
            compRef.ConditionData = compData.ConditionData;
            compRef.IsBroadcastable = compData.IsBroadcastable;
            compRef.Broadcast = compData.Broadcast;
            compRef.IsTargeted = compData.IsTargeted;
            compRef.TargetId = compData.TargetId;
            compRef.refObj = tuple.obj;
            var instance = runtimeSystem.AddRunningInstance<DestroyOnSystem>();
            instance.Init(runtimeSystem.World, tuple.id);
        }
    }
}