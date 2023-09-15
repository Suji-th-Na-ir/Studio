using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Collide")]
    public class Collide : MonoBehaviour, IComponent
    {
        public enum DestroyOnEnum
        {
            [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other")]
            OnObjectCollide
        }

        public Atom.StartOn startOn = new();
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        [AliasDrawer("Broadcast")]
        public string broadcast = null;
        [AliasDrawer("Do\nAlways")]
        public bool executeMultipleTimes = true;

        public void Awake()
        {
            playSFX.Setup<Collide>(gameObject);
            playVFX.Setup<Collide>(gameObject);
        }

        public (string type, string data) Export()
        {
            var start = Helper.GetEnumValueByIndex<DestroyOnEnum>(startOn.data.startIndex);
            var data = new CollideComponent()
            {
                canPlaySFX = playSFX.data.canPlay,
                sfxName = playSFX.data.clipName,
                sfxIndex = playSFX.data.clipIndex,
                canPlayVFX = playVFX.data.canPlay,
                vfxName = playVFX.data.clipName,
                vfxIndex = playVFX.data.clipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(start),
                ConditionData = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(start),
                startIndex = startOn.data.startIndex,
                startName = startOn.data.startName,
                listen = executeMultipleTimes ? Listen.Always : Listen.Once
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<CollideComponent>(data.data);
            playSFX.data.canPlay = obj.canPlaySFX;
            playSFX.data.clipName = obj.sfxName;
            playSFX.data.clipIndex = obj.sfxIndex;
            playVFX.data.canPlay = obj.canPlayVFX;
            playVFX.data.clipName = obj.vfxName;
            playVFX.data.clipIndex = obj.vfxIndex;
            broadcast = obj.Broadcast;
            startOn.data.startIndex = obj.startIndex;
            startOn.data.startName = obj.startName;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, broadcast, null);
        }
    }
}
