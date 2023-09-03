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

            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(Type.data.broadcast, ""
                               , new ComponentDisplayDock() { componentGameObject = gameObject, componentType = this.GetType().Name });
        }

        public (string type, string data) Export()
        {
            var comp = new RotateComponent
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

            comp.ConditionType = GetStartEvent();
            comp.ConditionData = GetStartCondition();
            comp.listenIndex = startOn.data.listenIndex;
            List<Axis> axes = new List<Axis>();
            if (Type.data.Xaxis)
                axes.Add(Axis.X);
            if (Type.data.Yaxis)
                axes.Add(Axis.Y);
            if (Type.data.Zaxis)
                axes.Add(Axis.Z);

            comp.axis = axes.ToArray();

            ModifyDataAsPerSelected(ref comp);
            gameObject.TrySetTrigger(false, true);
            string type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp, Formatting.Indented);
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
            RotateComponent comp = JsonConvert.DeserializeObject<RotateComponent>($"{cdata.data}");
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;

            for (int i = 0; i < comp.axis.Length; i++)
            {
                if (comp.axis[i] == Axis.X)
                    Type.data.Xaxis = true;
                if (comp.axis[i] == Axis.Y)
                    Type.data.Yaxis = true;
                if (comp.axis[i] == Axis.Z)
                    Type.data.Zaxis = true;
            }
            Type.data.direction = comp.direction;
            Type.data.rotateType = (int)comp.rotationType;
            Type.data.speed = comp.speed;
            Type.data.degrees = comp.rotateBy;
            Type.data.pauseBetween = comp.pauseFor;
            Type.data.repeat = comp.repeatFor;
            Type.data.broadcast = comp.Broadcast;
            Type.data.broadcastAt = comp.broadcastAt;
            Type.data.listen = comp.listen;

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                startOn.data.startIndex = (int)(StartOn)result;
            }

            if (comp.ConditionType.ToLower().Contains("listen"))
            {
                EditorOp.Resolve<DataProvider>().AddToListenList(GetInstanceID()+"_rotate",comp.ConditionData);
            }
            startOn.data.listenIndex = comp.listenIndex;
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
    }
}

