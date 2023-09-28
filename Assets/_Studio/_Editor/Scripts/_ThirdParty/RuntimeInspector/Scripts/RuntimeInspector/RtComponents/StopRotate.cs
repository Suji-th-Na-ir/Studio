using System;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.StopRotate"), AliasDrawer("Stop Rotate")]
    public class StopRotate : BaseBehaviour
    {
        public enum StartOn
        {
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
            OnClick,
            [EditorEnumField("Terra.Studio.Listener"), AliasDrawer("Broadcast Listened")]
            BroadcastListen,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
            OnObjectCollide,

        }
        [AliasDrawer("StopWhen")]
        public Atom.StartOn startOn = new();
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string broadcast = null;
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();

        protected override string ComponentName => nameof(StopRotate);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcast
        };
        protected override string[] ListenerRefs => new string[]
        {
            startOn.data.listenName
        };

        protected override void Awake()
        {
            base.Awake();
            startOn.Setup<StartOn>(gameObject, ComponentName, OnListenerUpdated, startOn.data.startIndex == 1);
            playSFX.Setup<StopRotate>(gameObject);
            playVFX.Setup<StopRotate>(gameObject);
        }

        public override (string type, string data) Export()
        {
            var data = new StopRotateComponent
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(GetEnum(startOn.data.startName)),
                ConditionData = GetConditionValue(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                startIndex = startOn.data.startIndex,
                canPlaySFX = playSFX.data.canPlay,
                sfxName = playSFX.data.clipName,
                sfxIndex = playSFX.data.clipIndex,
                canPlayVFX = playVFX.data.canPlay,
                vfxName = playVFX.data.clipName,
                vfxIndex = playVFX.data.clipIndex
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        private StartOn GetEnum(string data)
        {
            if (Enum.TryParse(data, out StartOn startOn))
            {
                return startOn;
            }
            return StartOn.OnClick;
        }

        private string GetConditionValue()
        {
            if (string.IsNullOrEmpty(startOn.data.startName) ||
                startOn.data.startName.Equals(StartOn.OnClick))
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOn.OnClick);
            }
            else if (startOn.data.startName.Equals(StartOn.OnObjectCollide.ToString()))
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOn.OnObjectCollide);
            }
            else
            {
                return startOn.data.listenName;
            }
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<StopRotateComponent>(data.data);
            startOn.data.startName = GetStart(obj).ToString();
            startOn.data.listenName = GetListenValues(obj);
            startOn.data.startIndex = obj.startIndex;
            broadcast = obj.Broadcast;
            playSFX.data.canPlay = obj.canPlaySFX;
            playSFX.data.clipIndex = obj.sfxIndex;
            playSFX.data.clipName = obj.sfxName;
            playVFX.data.canPlay = obj.canPlayVFX;
            playVFX.data.clipIndex = obj.vfxIndex;
            playVFX.data.clipName = obj.vfxName;
            var listenString = "";
            if (startOn.data.startIndex == 1)
                listenString = startOn.data.listenName;
            ImportVisualisation(broadcast, listenString);
        }

        private StartOn GetStart(StopRotateComponent comp)
        {
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                return (StartOn)result;
            }
            return StartOn.OnClick;
        }

        private string GetListenValues(StopRotateComponent comp)
        {
            if (comp.ConditionType.Equals(EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.BroadcastListen)))
            {
                return comp.ConditionData;
            }
            return null;
        }
    }
}
