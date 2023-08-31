using System;
using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Rotate")]
    public class Rotate : MonoBehaviour, IComponent
    {
        public Atom.StartOn startOn = new();
        public Atom.Rotate Type = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();

        private void Awake()
        {
            Type.referenceGO = gameObject;
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOn>(), this.GetType().Name);
            PlaySFX.Setup<Rotate>(gameObject);
            PlayVFX.Setup<Rotate>(gameObject);
        }

        public (string type, string data) Export()
        {
            var rc = new RotateComponent
            {
                direction = Type.data.direction,
                rotationType = (RotationType)Type.data.rotateType,
                repeatType = GetRepeatType(Type.data.repeat),
                speed = Type.data.speed,
                rotateBy = Type.data.degrees,
                pauseFor = Type.data.pauseBetween,
                repeatFor = Type.data.repeat,


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
                listen = Type.data.listen
            };

            rc.ConditionType = GetStartEvent();
            rc.ConditionData = GetStartCondition();
            rc.listenIndex = startOn.data.listenIndex;
            List<Axis> axes = new List<Axis>();
            if (Type.data.Xaxis)
                axes.Add(Axis.X);
            if (Type.data.Yaxis)
                axes.Add(Axis.Y);
            if (Type.data.Zaxis)
                axes.Add(Axis.Z);

            rc.axis = axes.ToArray();

            ModifyDataAsPerSelected(ref rc);
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
            RotateComponent cc = JsonConvert.DeserializeObject<RotateComponent>($"{cdata.data}");
            PlaySFX.data.canPlay = cc.canPlaySFX;
            PlaySFX.data.clipIndex = cc.sfxIndex;
            PlaySFX.data.clipName = cc.sfxName;
            PlayVFX.data.canPlay = cc.canPlayVFX;
            PlayVFX.data.clipIndex = cc.vfxIndex;
            PlayVFX.data.clipName = cc.vfxName;

            for (int i = 0; i < cc.axis.Length; i++)
            {
                if (cc.axis[i] == Axis.X)
                    Type.data.Xaxis = true;
                if (cc.axis[i] == Axis.Y)
                    Type.data.Yaxis = true;
                if (cc.axis[i] == Axis.Z)
                    Type.data.Zaxis = true;
            }
            Type.data.direction = cc.direction;
            Type.data.rotateType = (int)cc.rotationType;
            Type.data.speed = cc.speed;
            Type.data.degrees = cc.rotateBy;
            Type.data.pauseBetween = cc.pauseFor;
            Type.data.repeat = cc.repeatFor;
            Type.data.broadcast = cc.Broadcast;
            Type.data.broadcastAt = cc.broadcastAt;
            Type.data.listen = cc.listen;

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(StartOn), out object result))
            {
                startOn.data.startIndex = (int)(StartOn)result;
            }

            if (cc.ConditionType.ToLower().Contains("listen"))
            {
                EditorOp.Resolve<DataProvider>().AddToListenList(GetInstanceID()+"_rotate",cc.ConditionData);
            }
            startOn.data.listenIndex = cc.listenIndex;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, this.GetType().Name, Type.data.broadcast, Type.data.listenTo);
        }

        private void ModifyDataAsPerSelected(ref RotateComponent component)
        {
            switch (component.rotationType)
            {
                case RotationType.RotateForever:
                case RotationType.OscillateForever:
                case RotationType.IncrementallyRotateForever:
                    component.repeatFor = int.MaxValue;
                    break;
            }
            if (component.rotationType == RotationType.RotateForever) component.rotateBy = 360f;
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

