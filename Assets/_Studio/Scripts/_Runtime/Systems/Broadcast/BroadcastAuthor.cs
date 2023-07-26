using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    public class BroadcastAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var tuple = ((int id, string type, string compData, GameObject obj))data;
            var broadcastCompData = JsonConvert.DeserializeObject<BroadcastComponent>(tuple.compData);
            var ecsWorld = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<BroadcastComponent>();
            compPool.Add(tuple.id);
            ref var compRef = ref compPool.Get(tuple.id);
            compRef.reference = tuple.obj;
            compRef.Broadcast = broadcastCompData.Broadcast;
            compRef.IsBroadcastable = broadcastCompData.IsBroadcastable;
            compRef.IsConditionAvailable = broadcastCompData.IsConditionAvailable;
            compRef.ConditionType = broadcastCompData.ConditionType;
            compRef.ConditionData = broadcastCompData.ConditionData;
            compRef.IsTargeted = broadcastCompData.IsTargeted;
            Interop<RuntimeInterop>.Current.Resolve<BroadcastSystem>().Init(ecsWorld);
        }
    }
}
