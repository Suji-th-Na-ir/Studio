using System;
using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace RuntimeInspectorNamespace
{
    [Serializable]
    public class RotateComponentData
    {
        public int rotateType;
        public Axis axis;
        public Direction direction;
        public float degrees = 0f;
        public float speed = 0f;
        public int repeat = 0;
        public float pauseBetween = 0f;
        public string broadcast = "";
        public BroadcastAt broadcastAt;
    }
    
    [EditorDrawComponent("Terra.Studio.Rotate")]
    public class Rotate : MonoBehaviour, IComponent
    {
        public StartOn Start = StartOn.GameStart;
        public Atom.Rotate Type = new Atom.Rotate();
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        private RotateComponent rComp;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) Export();
        }

        public (string type, string data) Export()
        {
            RotateComponent rc = new RotateComponent
            {
                axis = Type.data.axis,
                direction = Type.data.direction,
                rotationType = (RotationType)Type.data.rotateType,
                repeatType = GetRepeatType(Type.data.repeat),
                speed = Type.data.speed,
                rotateBy = Type.data.degrees,
                pauseFor = Type.data.pauseBetween,
                repeatFor = Type.data.repeat,
                
                IsBroadcastable = true,
                broadcastAt = Type.data.broadcastAt,
                Broadcast = Type.data.broadcast,
                
                canPlaySFX = PlaySFX.canPlay,
                canPlayVFX = PlayVFX.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.clipName) ? null : PlaySFX.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.clipName) ? null : PlayVFX.clipName,
                sfxIndex = PlaySFX.clipIndex,
                vfxIndex = PlayVFX.clipIndex,

                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(Start)
            };

            ModifyDataAsPerSelected(ref rc);
            string type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(rc, Formatting.Indented);
            return (type, data);
        }
        

        private RepeatType GetRepeatType(float _value)
        {
            if (_value == 0) return RepeatType.Forever;
            else return RepeatType.XTimes;
        }

        public string GetStartEvent()
        {
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(Start);
            return eventName;
        }

       

        private StartOn GetStartCondition(string _name)
        {
            if (_name == StartOn.GameStart.ToString()) return StartOn.GameStart;
            else if (_name == StartOn.OnPlayerCollide.ToString()) return StartOn.OnPlayerCollide;
            else if (_name == StartOn.OnClick.ToString()) return StartOn.OnClick;
            return StartOn.GameStart;
        }

        public void Import(EntityBasedComponent cdata)
        {
            RotateComponent cc = JsonConvert.DeserializeObject<RotateComponent>($"{cdata.data}");
            PlaySFX.canPlay = cc.canPlaySFX;
            PlaySFX.clipIndex = cc.sfxIndex;
            PlaySFX.clipName = cc.sfxName;
            PlayVFX.canPlay = cc.canPlayVFX;
            PlayVFX.clipIndex = cc.vfxIndex;
            PlayVFX.clipName = cc.vfxName;

            Type.data.axis = cc.axis;
            Type.data.direction = cc.direction;
            Type.data.rotateType = (int)cc.rotationType;
            Type.data.speed = cc.speed;
            Type.data.degrees = cc.rotateBy;
            Type.data.pauseBetween = cc.pauseFor;
            Type.data.repeat = cc.repeatFor;
            Type.data.broadcast = cc.Broadcast;
            Type.data.broadcastAt = cc.broadcastAt;
            
            Start = GetStartCondition(cc.ConditionType);
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
