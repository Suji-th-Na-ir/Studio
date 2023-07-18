using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    public class OscillateAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var tuple = ((int id, string type, string compData, GameObject obj))data;
            var entity = tuple.id;
            var jString = tuple.compData;
            var go = tuple.obj;
            var oscillateCompData = JsonConvert.DeserializeObject<OscillateComponent>(jString);
            var ecsWorld = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
            var compPool = ecsWorld.GetPool<OscillateComponent>();
            compPool.Add(entity);
            ref var compRef = ref compPool.Get(entity);
            compRef.fromPoint = oscillateCompData.fromPoint;
            compRef.toPoint = oscillateCompData.toPoint;
            compRef.loop = oscillateCompData.loop;
            compRef.oscillatableTr = tuple.obj.transform;
            compRef.speed = 1.5f;
            compRef.IsConditionAvailable = oscillateCompData.IsConditionAvailable;
            compRef.IsExecuted = oscillateCompData.IsExecuted;
            compRef.ConditionType = oscillateCompData.ConditionType;
            compRef.ConditionData = oscillateCompData.ConditionData;
            Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().AddUpdateSystem(new OscillateSystem());
        }
    }
}