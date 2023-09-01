using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.Oscillate")]
    public class OscillateAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var authorData = (ComponentAuthorData)data;
            var oscillateCompData = JsonConvert.DeserializeObject<OscillateComponent>(authorData.compData);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<OscillateComponent>();
            compPool.Add(authorData.entity);
            ref var compRef = ref compPool.Get(authorData.entity);
            Helper.CopyStructFieldValues(oscillateCompData, ref compRef);
            compRef.ConditionData = oscillateCompData.ConditionData;
            compRef.ConditionType = oscillateCompData.ConditionType;
            compRef.IsConditionAvailable = oscillateCompData.IsConditionAvailable;
            compRef.IsBroadcastable = oscillateCompData.IsBroadcastable;
            compRef.Broadcast = oscillateCompData.Broadcast;
            compRef.oscillatableTr = authorData.obj.transform;
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<OscillateSystem>();
            instance.Init(ecsWorld, authorData.entity);
        }
    }
}