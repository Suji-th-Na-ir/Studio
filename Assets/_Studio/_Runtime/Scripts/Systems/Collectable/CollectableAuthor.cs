using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.Collectable")]
    public class CollectableAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var collectableCompData = JsonConvert.DeserializeObject<CollectableComponent>(authorData.compData);
            var runtimeSystem = RuntimeOp.Resolve<RuntimeSystem>();
            var ecsWorld = runtimeSystem.World;
            var compPool = ecsWorld.GetPool<CollectableComponent>();
            compPool.Add(authorData.entity);
            ref var compRef = ref compPool.Get(authorData.entity);
            Helper.CopyStructFieldValues(collectableCompData, ref compRef);
            compRef.CanExecute = collectableCompData.CanExecute;
            compRef.IsConditionAvailable = collectableCompData.IsConditionAvailable;
            compRef.ConditionType = collectableCompData.ConditionType;
            compRef.ConditionData = collectableCompData.ConditionData;
            compRef.IsBroadcastable = collectableCompData.IsBroadcastable;
            compRef.Broadcast = collectableCompData.Broadcast;
            compRef.IsTargeted = collectableCompData.IsTargeted;
            compRef.TargetId = collectableCompData.TargetId;
            compRef.refObject = authorData.obj;
            var instance = runtimeSystem.AddRunningInstance<CollectableSystem>();
            instance.Init(ecsWorld, authorData.entity);
        }
    }
}