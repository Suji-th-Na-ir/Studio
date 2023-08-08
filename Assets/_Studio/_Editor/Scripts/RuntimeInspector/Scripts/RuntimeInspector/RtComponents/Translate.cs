using System;
using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;
using Terra.Studio.RTEditor;

namespace RuntimeInspectorNamespace
{
    [Serializable]
    public class TranslateComponentData
    {
        public int translateType;
        public Vector3 moveTo = Vector3.zero;
        public float pauseFor = 0f;
        public float speed = 0f;
        public int repeat = 0;
        public string broadcast = "";
        public BroadcastAt broadcastAt;
        public Vector3 targetPosition;
        public string listenTo;
    }
    
    [EditorDrawComponent("Terra.Studio.Translate")]
    public class Translate : MonoBehaviour, IComponent
    {
        public StartOn start = StartOn.GameStart;
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
                repeatFor = Type.data.repeat,
                targetPosition = Type.data.moveTo,
                startPosition = transform.position,

                IsBroadcastable = !string.IsNullOrEmpty(Type.data.broadcast),
                broadcastAt = Type.data.broadcastAt,


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

            ModifyDataAsPerGiven(ref rc);
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
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(start);
            return eventName;
        }

        public string GetStartCondition()
        {
            if (start == StartOn.OnPlayerCollide)
                return "Player";
            else if (start == StartOn.OnClick)
                return "OnClick";
            else if (start == StartOn.GameStart)
                return "Start";
            else if (start == StartOn.BroadcastListen)
                return Type.data.listenTo;

            return "";
        }

        private StartOn GetStartCondition(string _name)
        {

            if (_name == "Terra.Studio.GameStart") return StartOn.GameStart;
            else if (_name == "Terra.Studio.TriggerAction") return StartOn.OnPlayerCollide;
            else if (_name == "Terra.Studio.MouseAction") return StartOn.OnClick;
            else if (_name == "Terra.Studio.Listener") return StartOn.BroadcastListen;
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
            Type.data.moveTo = cc.targetPosition;
            Type.data.repeat = cc.repeatFor;
            Type.data.broadcast = cc.Broadcast;
            Type.data.broadcastAt = cc.broadcastAt;
            Type.data.listenTo = cc.ConditionData;

            start = GetStartCondition(cc.ConditionType);
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
    }
}