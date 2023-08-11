using System;
using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Translate")]
    public class Translate : MonoBehaviour, IComponent
    {
        public StartOn start = StartOn.GameStart;
        public Atom.Translate Type = new ();
        public Atom.PlaySfx PlaySFX = new ();
        public Atom.PlayVfx PlayVFX = new ();
        private RotateComponent rComp;
        
        public void Start()
        {
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
