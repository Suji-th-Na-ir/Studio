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
        public Atom.Broadcast broadcastData = new();
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();

        public override string ComponentName => nameof(StopRotate);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastData.broadcast
        };
        protected override string[] ListenerRefs => new string[]
        {
            startOn.data.listenName
        };
        protected override Atom.PlaySfx[] Sfxes => new Atom.PlaySfx[]
        {
            playSFX
        };
        protected override Atom.PlayVfx[] Vfxes => new Atom.PlayVfx[]
        {
            playVFX
        };

        protected override void Awake()
        {
            base.Awake();
            startOn.Setup<StartOn>(gameObject, ComponentName, OnListenerUpdated, startOn.data.startIndex == 1);
            playSFX.Setup<StopRotate>(gameObject);
            playVFX.Setup<StopRotate>(gameObject);
            broadcastData.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            var data = new StopRotateComponent
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(GetEnum(startOn.data.startName)),
                ConditionData = GetConditionValue(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                startIndex = startOn.data.startIndex,
                FXData = GetFXData(),
                Listen = Listen.Always
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
            broadcastData.broadcast = obj.Broadcast;
            MapSFXAndVFXData(obj.FXData);
            var listenString = "";
            if (startOn.data.startIndex == 1)
                listenString = startOn.data.listenName;
            ImportVisualisation(broadcastData.broadcast, listenString);
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
