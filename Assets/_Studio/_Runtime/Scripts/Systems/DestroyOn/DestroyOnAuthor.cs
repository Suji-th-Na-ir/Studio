using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.DestroyOn")]
    public class DestroyOnAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var compData = JsonConvert.DeserializeObject<DestroyOnComponent>(authorData.compData);
            var runtimeSystem = RuntimeOp.Resolve<RuntimeSystem>();
            var compPool = runtimeSystem.World.GetPool<DestroyOnComponent>();
            compPool.Add(authorData.entity);
            ref var compRef = ref compPool.Get(authorData.entity);
            Helper.CopyStructFieldValues(compData, ref compRef);
            compRef.CanExecute = compData.CanExecute;
            compRef.IsConditionAvailable = compData.IsConditionAvailable;
            compRef.ConditionType = compData.ConditionType;
            compRef.ConditionData = compData.ConditionData;
            compRef.IsBroadcastable = compData.IsBroadcastable;
            compRef.Broadcast = compData.Broadcast;
            compRef.IsTargeted = compData.IsTargeted;
            compRef.TargetId = compData.TargetId;
            compRef.refObj = authorData.obj;
            var instance = runtimeSystem.AddRunningInstance<DestroyOnSystem>();
            instance.Init(runtimeSystem.World, authorData.entity);
        }
    }
}