using System;
using UnityEngine;
using PlayShifu.Terra;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.SetObjectPosition")]
    public class SetObjectPosition : MonoBehaviour, IComponent
    {
        public enum StartOptions
        {
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
            OnClick,
            [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Any")]
            OnObjectCollide,
            [EditorEnumField("Terra.Studio.Listener")]
            BroadcastListen
        }

        private readonly Vector3 INFINITY = new(-float.MaxValue, -float.MaxValue, -float.MaxValue);
        public Atom.StartOn startOn = new();
        public Vector3 targetPosition = new(-float.MaxValue, -float.MaxValue, -float.MaxValue);
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        public string broadcast;

        private void Awake()
        {
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOptions>(), GetType().Name);
            playSFX.Setup<SetObjectPosition>(gameObject);
            playVFX.Setup<SetObjectPosition>(gameObject);
        }

        private void Start()
        {
            if (targetPosition == INFINITY)
            {
                var newTarget = transform.position;
                newTarget.z += 5f;
                targetPosition = newTarget;
            }
        }

        public (string type, string data) Export()
        {
            var data = new SetObjectPositionComponent()
            {
                IsConditionAvailable = true,
                ConditionType = GetConditionType(),
                ConditionData = GetConditionData(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                targetPosition = targetPosition,
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

        public void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<SetObjectPositionComponent>(data.data);
            broadcast = obj.Broadcast;
            targetPosition = obj.targetPosition;
            AssignStartOnData(obj);
            AssignSFXandVFXData(obj);
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(
                gameObject,
                GetType().Name,
                broadcast,
                startOn.data.listenName);
        }

        private void AssignStartOnData(SetObjectPositionComponent comp)
        {
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOptions), out var result))
            {
                var startOn = (StartOptions)result;
                var startName = startOn.ToString();
                if (startOn == StartOptions.OnPlayerCollide || startOn == StartOptions.OnObjectCollide)
                {
                    if (comp.ConditionType.Equals("Player"))
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
