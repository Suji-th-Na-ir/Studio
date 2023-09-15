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
        [DisplayName("RotateWhen")]
        public Atom.StartOn startOn = new();
        public Atom.Rotate Type = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();

        private string guid;

        private void Awake()
        {
            guid = GetInstanceID() + "_rotate";
            Type.Setup(guid, gameObject, GetType().Name);
            startOn.Setup(gameObject, Helper.GetEnumWithDisplayNames<StartOn>(), this.GetType().Name,startOn.data.startIndex==3);
            PlaySFX.Setup<Rotate>(gameObject);
            PlayVFX.Setup<Rotate>(gameObject);
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
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                broadcastAt = Type.data.broadcastAt,
                IsBroadcastable = !string.IsNullOrEmpty(Type.data.broadcast),
                Broadcast = string.IsNullOrEmpty(Type.data.broadcast) ? null : Type.data.broadcast,
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,
                listen = Type.data.listen
            };

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

        public string GetStartEvent()
        {
            int index = startOn.data.startIndex;
            var value = (StartOn)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }


        public string GetStartCondition()
        {
            int index = startOn.data.startIndex;
            var value = (StartOn)index;
            string inputString = value.ToString();
            if (inputString.ToLower().Contains("listen"))
            {
                return startOn.data.listenName;
            }
            var data = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            return data;
        }

        private RepeatType GetRepeatType(float _value)
        {
            if (_value == 0) return RepeatType.Forever;
            else return RepeatType.XTimes;
        }


        public void Import(EntityBasedComponent cdata)
        {
            RotateComponent comp = JsonConvert.DeserializeObject<RotateComponent>(cdata.data);
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
            Type.data.broadcastAt = comp.broadcastAt;
            Type.data.broadcast = comp.Broadcast;
            Type.data.listen = comp.listen;
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                startOn.data.startIndex = (int)(StartOn)result;
            }
            startOn.data.startName = comp.ConditionType;
            startOn.data.listenName = comp.ConditionData;
            var listenString = "";
            if (startOn.data.startIndex == 3)
                listenString = startOn.data.listenName;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Type.data.broadcast,listenString);
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

