using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;
using PlayShifu.Terra;
using System;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Translate")]
    public class Translate : MonoBehaviour, IComponent
    {
        public Atom.StartOn startOn = new();
        public Atom.Translate Type = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        private RotateComponent rComp;

        private void Awake()
        {
            Type.referenceGO = gameObject;
        }

        public void Start()
        {
            Type.data.moveTo = transform.localPosition;
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOn>());
            PlaySFX.Setup(gameObject);
            PlayVFX.Setup(gameObject);
        }

        public (string type, string data) Export()
        {
            TranslateComponent rc = new TranslateComponent
            {
                translateType = (TranslateType)Type.data.translateType,
                speed = Type.data.speed,
                pauseFor = Type.data.pauseFor,
                repeatFor = Type.data.repeat,
                targetPosition = Type.data.moveTo,
                startPosition = transform.position,

                IsBroadcastable = !string.IsNullOrEmpty(Type.data.broadcast),
                broadcastAt = Type.data.broadcastAt,


                Broadcast = Type.data.broadcast,
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,

                IsConditionAvailable = true,
                listen = Type.data.listen,
                
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                listenIndex = startOn.data.listenIndex
            };

            ModifyDataAsPerGiven(ref rc);
            gameObject.TrySetTrigger(false, true);
            string type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(rc, Formatting.Indented);
            return (type, data);
        }
        
        public string GetStartEvent(string _input = null)
        {
            int index = startOn.data.startIndex;
            string inputString = ((StartOn)index).ToString();
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (Enum.TryParse(inputString, out StartOn enumValue))
            {
                var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(enumValue);
                return eventName;
            }
            return EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.OnClick);
        }


        public string GetStartCondition(string _input = null)
        {
            int index = startOn.data.startIndex;
            string inputString = ((StartOn)index).ToString();
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (inputString.ToLower().Contains("listen"))
            {
                return EditorOp.Resolve<DataProvider>().GetListenString(startOn.data.listenIndex);
            }
            else
            {
                if (Enum.TryParse(inputString, out StartOn enumValue))
                {
                    return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(enumValue);
                }
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOn.GameStart);
            }
        }


        private RepeatType GetRepeatType(float _value)
        {
            if (_value == 0) return RepeatType.Forever;
            else return RepeatType.XTimes;
        }

        public void Import(EntityBasedComponent cdata)
        {
            TranslateComponent cc = JsonConvert.DeserializeObject<TranslateComponent>($"{cdata.data}");
            PlaySFX.data.canPlay = cc.canPlaySFX;
            PlaySFX.data.clipIndex = cc.sfxIndex;
            PlaySFX.data.clipName = cc.sfxName;
            PlayVFX.data.canPlay = cc.canPlayVFX;
            PlayVFX.data.clipIndex = cc.vfxIndex;
            PlayVFX.data.clipName = cc.vfxName;

            Type.data.translateType = (int)cc.translateType;

            Type.data.speed = cc.speed;
            Type.data.pauseFor = cc.pauseFor;
            Type.data.moveTo = cc.targetPosition;
            Type.data.repeat = cc.repeatFor;
            Type.data.broadcast = cc.Broadcast;
            Type.data.broadcastAt = cc.broadcastAt;
            Type.data.listenTo = cc.ConditionData;
            Type.data.listen = cc.listen;

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(StartOn), out object result))
            {
                startOn.data.startIndex = (int)(StartOn)result;
            }

            if (cc.ConditionType.ToLower().Contains("listen"))
            {
                EditorOp.Resolve<DataProvider>().AddToListenList(GetInstanceID()+"_translate",cc.ConditionData);
            }
            startOn.data.listenIndex = cc.listenIndex;
            start = GetStartCondition(cc.ConditionType);
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, this.GetType().Name, Type.data.broadcast, Type.data.listenTo);
        }

        private void ModifyDataAsPerGiven(ref TranslateComponent component)
        {
            switch (component.translateType)
            {
                case TranslateType.MoveForever:
                case TranslateType.MoveIncrementallyForever:
                case TranslateType.PingPongForever:
                    component.repeatFor = int.MaxValue;
                    break;
                default:
                    component.repeatFor = Type.data.repeat;
                    break;
            }
        }

        private void OnDestroy()
        {
            if (gameObject.TryGetComponent(out Collider collider) && !gameObject.TryGetComponent(out MeshRenderer _))
            {
                Destroy(collider);
            }
        }
    }
}
