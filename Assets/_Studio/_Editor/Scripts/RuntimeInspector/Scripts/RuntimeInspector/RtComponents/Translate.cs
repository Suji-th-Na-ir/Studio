using System;
using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace RuntimeInspectorNamespace
{
    [Serializable]
    public class TranslateComponentData
    {
        public int translateType;
        public float pauseAtDistance = 0f;
        public float pauseFor = 0f;
        public float speed = 0f;
        public int repeat = 0;
        public string broadcast = "";
        public BroadcastAt broadcastAt;
        public Vector3 targetPosition;
    }
    
    [EditorDrawComponent("Terra.Studio.Translate")]
    public class Translate : MonoBehaviour, IComponent
    {
        public StartOn Start = StartOn.GameStart;
        public Atom.Translate Type = new Atom.Translate();
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        private RotateComponent rComp;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) Export();
        }

        public (string type, string data) Export()
        {
            TranslateComponent rc = new TranslateComponent
            {
                translateType = (TranslateType)Type.data.translateType,
                speed = Type.data.speed,
                pauseFor = Type.data.pauseFor,
                pauseAtDistance = Type.data.pauseAtDistance,
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
                ConditionData = GetStartCondition()
            };

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
            TranslateComponent cc = JsonConvert.DeserializeObject<TranslateComponent>($"{cdata.data}");
            PlaySFX.canPlay = cc.canPlaySFX;
            PlaySFX.clipIndex = cc.sfxIndex;
            PlaySFX.clipName = cc.sfxName;
            PlayVFX.canPlay = cc.canPlayVFX;
            PlayVFX.clipIndex = cc.vfxIndex;
            PlayVFX.clipName = cc.vfxName;

            Type.data.translateType = (int)cc.translateType;


            Type.data.speed = cc.speed;
            Type.data.pauseFor = cc.pauseFor;
            Type.data.pauseAtDistance = cc.pauseAtDistance;
            Type.data.repeat = cc.repeatFor;
            Type.data.broadcast = cc.Broadcast;
            Type.data.broadcastAt = cc.broadcastAt;
            
            Start = GetStartCondition(cc.ConditionType);
        }
    }
}
