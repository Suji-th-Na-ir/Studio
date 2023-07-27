using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.Collectable")]
    public class CollectableAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var tuple = ((int id, string type, string compData, GameObject obj))data;
            var collectableCompData = JsonConvert.DeserializeObject<CollectableComponent>(tuple.compData);
            var runtimeSystem = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>();
            var ecsWorld = runtimeSystem.World;
            var compPool = ecsWorld.GetPool<CollectableComponent>();
            compPool.Add(tuple.id);
            ref var compRef = ref compPool.Get(tuple.id);
            Helper.CopyStructFieldValues(collectableCompData, ref compRef);
            compRef.CanExecute = collectableCompData.CanExecute;
            compRef.IsConditionAvailable = collectableCompData.IsConditionAvailable;
            compRef.ConditionType = collectableCompData.ConditionType;
            compRef.ConditionData = collectableCompData.ConditionData;
            compRef.IsBroadcastable = collectableCompData.IsBroadcastable;
            compRef.Broadcast = collectableCompData.Broadcast;
            compRef.IsTargeted = collectableCompData.IsTargeted;
            compRef.TargetId = collectableCompData.TargetId;
            compRef.refObject = tuple.obj;
            var instance = runtimeSystem.AddRunningInstance<CollectableSystem>();
            instance.Init(ecsWorld, tuple.id);
        }
    }
}