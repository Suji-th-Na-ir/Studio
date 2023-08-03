using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [Author("Terra.Studio.Oscillate")]
    public class OscillateAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var (id, _, compData, obj) = ((int, string, string, GameObject))data;
            var entity = id;
            var jString = compData;
            var oscillateCompData = JsonConvert.DeserializeObject<OscillateComponent>(jString);
            var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<OscillateComponent>();
            compPool.Add(entity);
            ref var compRef = ref compPool.Get(entity);
            Helper.CopyStructFieldValues(oscillateCompData, ref compRef);
            compRef.ConditionData = oscillateCompData.ConditionData;
            compRef.ConditionType = oscillateCompData.ConditionType;
            compRef.IsConditionAvailable = oscillateCompData.IsConditionAvailable;
            compRef.IsBroadcastable = oscillateCompData.IsBroadcastable;
            compRef.Broadcast = oscillateCompData.Broadcast;
            compRef.oscillatableTr = obj.transform;
            var instance = RuntimeOp.Resolve<RuntimeSystem>().AddRunningInstance<OscillateSystem>();
            instance.Init(ecsWorld, entity);
        }
    }
}