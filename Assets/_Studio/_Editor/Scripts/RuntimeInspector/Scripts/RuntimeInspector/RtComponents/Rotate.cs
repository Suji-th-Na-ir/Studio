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
        public RotationType rotateType;
        public string axis = null;
        public string direction = null;
        public float degrees = 0f;
        public float speed = 0f;
        public int repeat = 0;
        public float pauseBetween = 0f;
    }
    
    [EditorDrawComponent("Terra.Studio.Rotate")]
    public class Rotate : MonoBehaviour, IComponent
    {
        public StartOn Start = StartOn.GameStart;
        public Atom.Rotate Type = new Atom.Rotate();
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public BroadcastAt BroadcastAt = BroadcastAt.End;
        public string Broadcast = null;
        private RotateComponent rComp;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) Export();
        }

        public (string type, string data) Export()
        {
            RotateComponent rc = new RotateComponent
            {
                IsBroadcastable = true,
                axis = GetAxis(Type.rData.axis),
                direction = GetDirection(Type.rData.direction),
                
                rotationType = Type.rData.rotateType,
                repeatType = GetRepeatType(Type.rData.repeat),
                speed = Type.rData.speed,
                rotateBy = Type.rData.degrees,
                pauseFor = Type.rData.pauseBetween,
                repeatFor = Type.rData.repeat,
                
                canPlaySFX = PlaySFX.canPlay,
                canPlayVFX = PlayVFX.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.clipName) ? null : PlaySFX.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.clipName) ? null : PlaySFX.clipName,
                sfxIndex = PlaySFX.clipIndex,
                vfxIndex = PlayVFX.clipIndex,
                IsConditionAvailable = false,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(Start),
                ConditionData = "Start"
            };

            rc.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
            rc.broadcastAt = BroadcastAt;
            rc.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;

            string type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(rc, Formatting.Indented);
            
            Debug.Log(data);
            
            return (type, data);
        }

        private RepeatType GetRepeatType(float _value)
        {
            if (_value == 0) return RepeatType.Forever;
            else return RepeatType.XTimes;
        }

        private Axis GetAxis(string _value)
        {
            if (_value == Axis.X.ToString()) return Axis.X;
            else if (_value == Axis.Y.ToString()) return Axis.Y;
            else if (_value == Axis.Z.ToString()) return Axis.Z;
            return Axis.X;
        }

        private Direction GetDirection(string _value)
        {
            if (_value == Direction.Clockwise.ToString()) return Direction.Clockwise;
            else if (_value == Direction.AntiClockwise.ToString()) return Direction.AntiClockwise;
            return Direction.Clockwise;
        }

        private StartOn GetStartCondition(string _value)
        {
            if (string.IsNullOrEmpty(_value)) return StartOn.None;
            else if (_value == StartOn.GameStart.ToString()) return StartOn.GameStart;
            else if (_value == StartOn.BroadcastListen.ToString()) return StartOn.BroadcastListen;
            else if (_value == StartOn.OnClick.ToString()) return StartOn.OnClick;
            else if (_value == StartOn.OnPlayerCollide.ToString()) return StartOn.OnPlayerCollide;
            else return StartOn.None;
        }

        public void Import(EntityBasedComponent cdata)
        {
            RotateComponent cc = JsonConvert.DeserializeObject<RotateComponent>($"{cdata.data}");
            BroadcastAt = cc.broadcastAt;
            Broadcast = cc.Broadcast;
            PlaySFX.canPlay = cc.canPlaySFX;
            PlaySFX.clipIndex = cc.sfxIndex;
            PlaySFX.clipName = cc.sfxName;
            PlayVFX.canPlay = cc.canPlayVFX;
            PlayVFX.clipIndex = cc.vfxIndex;
            PlayVFX.clipName = cc.vfxName;

            Type.rData.axis = cc.axis.ToString();
            Type.rData.direction = cc.direction.ToString();
            Type.rData.rotateType = cc.rotationType;
            Type.rData.speed = cc.speed;
            Type.rData.degrees = cc.rotateBy;
            Type.rData.pauseBetween = cc.pauseFor;
            Type.rData.repeat = cc.repeatFor;
            Start = GetStartCondition(cc.ConditionType);
        }
    }
}
