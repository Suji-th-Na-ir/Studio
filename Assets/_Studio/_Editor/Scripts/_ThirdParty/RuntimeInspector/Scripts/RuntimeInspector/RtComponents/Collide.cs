using Newtonsoft.Json;
using PlayShifu.Terra;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Collide")]
    public class Collide : BaseBehaviour
    {
        public enum DestroyOnEnum
        {
            [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
            OnObjectCollide
        }

        public Atom.StartOn startOn = new();
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string broadcast = null;

        protected override string ComponentName => nameof(Collide);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcast
        };

        //[AliasDrawer("Do\nAlways")]
        //public bool executeMultipleTimes = true;

        protected override void Awake()
        {
            base.Awake();
            startOn.Setup<DestroyOnEnum>(gameObject, ComponentName);
            playSFX.Setup<Collide>(gameObject);
            playVFX.Setup<Collide>(gameObject);
        }

        public override (string type, string data) Export()
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
                listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
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
            //executeMultipleTimes = obj.listen == Listen.Always;
            ImportVisualisation(broadcast, null);
        }
    }
}
