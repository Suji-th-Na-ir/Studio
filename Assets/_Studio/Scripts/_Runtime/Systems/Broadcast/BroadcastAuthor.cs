using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    public class BroadcastAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var tuple = ((int id, string type, string compData, GameObject obj))data;
            var entity = tuple.id;
            var jString = tuple.compData;
            var go = tuple.obj;
            var broadcastCompData = JsonConvert.DeserializeObject<BroadcastComponent>(jString);
            var ecsWorld = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<BroadcastComponent>();
            compPool.Add(entity);
            ref var compRef = ref compPool.Get(entity);
        }
    }
}
