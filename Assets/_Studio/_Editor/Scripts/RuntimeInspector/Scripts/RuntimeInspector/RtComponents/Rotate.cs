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
                axis = Type.rData.axis,
                direction = Type.rData.direction,
                rotationType = (RotationType)Type.rData.rotateType,
                repeatType = GetRepeatType(Type.rData.repeat),
                speed = Type.rData.speed,
                rotateBy = Type.rData.degrees,
                pauseFor = Type.rData.pauseBetween,
                repeatFor = Type.rData.repeat,
                
                IsBroadcastable = true,
                broadcastAt = Type.rData.broadcastAt,
                Broadcast = Type.rData.broadcast,
                
                canPlaySFX = PlaySFX.canPlay,
                canPlayVFX = PlayVFX.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.clipName) ? null : PlaySFX.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.clipName) ? null : PlayVFX.clipName,
                sfxIndex = PlaySFX.clipIndex,
                vfxIndex = PlayVFX.clipIndex,
                
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition()
            };

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

        public string GetStartCondition()
        {
            if (Start == StartOn.OnPlayerCollide)
                return "Player";

            if (Start == StartOn.OnClick)
                return "OnClick";

            if (Start == StartOn.GameStart)
                return "Start";

            return "";
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

            Type.rData.axis = cc.axis;
            Type.rData.direction = cc.direction;
            Type.rData.rotateType = (int)cc.rotationType;
            Type.rData.speed = cc.speed;
            Type.rData.degrees = cc.rotateBy;
            Type.rData.pauseBetween = cc.pauseFor;
            Type.rData.repeat = cc.repeatFor;
            Type.rData.broadcast = cc.Broadcast;
            Type.rData.broadcastAt = cc.broadcastAt;
            
            Start = GetStartCondition(cc.ConditionType);
        }
    }
}
