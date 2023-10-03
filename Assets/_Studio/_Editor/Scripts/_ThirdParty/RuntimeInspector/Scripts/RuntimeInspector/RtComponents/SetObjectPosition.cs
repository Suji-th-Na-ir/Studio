using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.SetObjectPosition"), AliasDrawer("Teleport Self")]
    public class SetObjectPosition : BaseBehaviour
    {
        public enum StartOptions
        {
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
            OnClick,
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
            OnObjectCollide,
            [EditorEnumField("Terra.Studio.Listener"), AliasDrawer("Broadcast Listened")]
            BroadcastListen
        }

        private readonly Vector3 INFINITY = new(-float.MaxValue, -float.MaxValue, -float.MaxValue);

        [AliasDrawer("TeleportWhen")]
        public Atom.StartOn startOn = new();
        [AliasDrawer("Target\nPosition")]
        public Atom.RecordedVector3 targetPosition = new(typeof(SetObjectPosition));
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string broadcast;

        private bool isGhostEnabled;
        private RecordVisualiser visualiser;

        public override string ComponentName => nameof(SetObjectPosition);
        public override Atom.RecordedVector3 RecordedVector3 => targetPosition;

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
            startOn.Setup<StartOptions>(gameObject, ComponentName, OnListenerUpdated, startOn.data.startIndex == 3);
            playSFX.Setup<SetObjectPosition>(gameObject);
            playVFX.Setup<SetObjectPosition>(gameObject);
        }

        private void OnGhostDataModified(object data)
        {
            var newVector = (Vector3)data;
            targetPosition.vector3 = newVector;
        }

        private void Start()
        {
            if (targetPosition.vector3 == INFINITY)
            {
                var newTarget = transform.position;
                newTarget.z += 5f;
                targetPosition.vector3 = newTarget;
            }
        }

        public override (string type, string data) Export()
        {
            var data = new SetObjectPositionComponent()
            {
                IsConditionAvailable = true,
                ConditionType = GetConditionType(),
                ConditionData = GetConditionData(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                targetPosition = targetPosition.vector3,
                startIndex = startOn.data.startIndex,
                canPlaySFX = playSFX.data.canPlay,
                sfxIndex = playSFX.data.clipIndex,
                sfxName = playSFX.data.clipName,
                canPlayVFX = playVFX.data.canPlay,
                vfxIndex = playVFX.data.clipIndex,
                vfxName = playVFX.data.clipName,
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        private string GetConditionType()
        {
            var type = GetEnum();
            var value = EditorOp.Resolve<DataProvider>().GetEnumValue(type);
            return value;
        }

        private string GetConditionData()
        {
            var type = GetEnum();
            if (type == StartOptions.BroadcastListen)
            {
                return startOn.data.listenName;
            }
            var value = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(type);
            return value;
        }

        private StartOptions GetEnum()
        {
            var selectedType = startOn.data.startName;
            var start = (StartOptions)Enum.Parse(typeof(StartOptions), selectedType);
            return start;
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<SetObjectPositionComponent>(data.data);
            broadcast = obj.Broadcast;
            targetPosition.vector3 = obj.targetPosition;
            AssignStartOnData(obj);
            AssignSFXandVFXData(obj);
            var listenString = "";
            if (startOn.data.startIndex == 3)
                listenString = startOn.data.listenName;
            ImportVisualisation(broadcast, listenString);
        }

        private void AssignStartOnData(SetObjectPositionComponent comp)
        {
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOptions), out var result))
            {
                var startOn = (StartOptions)result;
                var startName = startOn.ToString();
                if (startOn == StartOptions.OnPlayerCollide || startOn == StartOptions.OnObjectCollide)
                {
                    if (comp.ConditionData.Equals("Player"))
                    {
                        startName = StartOptions.OnPlayerCollide.ToString();
                    }
                    else
                    {
                        startName = StartOptions.OnObjectCollide.ToString();
                    }
                }
                this.startOn.data.startName = startName;
            }
            startOn.data.startIndex = comp.startIndex;
            startOn.data.listenName = comp.ConditionData;
        }

        private void AssignSFXandVFXData(SetObjectPositionComponent comp)
        {
            playSFX.data.canPlay = comp.canPlaySFX;
            playSFX.data.clipIndex = comp.sfxIndex;
            playSFX.data.clipName = comp.sfxName;
            playVFX.data.canPlay = comp.canPlayVFX;
            playVFX.data.clipIndex = comp.vfxIndex;
            playVFX.data.clipName = comp.vfxName;
        }
    }
}
