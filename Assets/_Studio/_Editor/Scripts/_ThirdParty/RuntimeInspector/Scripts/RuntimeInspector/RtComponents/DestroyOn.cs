using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;
using UnityEngine.Serialization;

namespace RuntimeInspectorNamespace
{
    public enum DestroyOnEnum
    {
        [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
        OnPlayerCollide,
        [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
        OnClick,
        [EditorEnumField("Terra.Studio.Listener")]
        BroadcastListen
    }
    
    [EditorDrawComponent("Terra.Studio.DestroyOn")]
    public class DestroyOn : MonoBehaviour, IComponent
    {
        public Atom.StartOn startOn = new ();
        public Atom.PlaySfx PlaySFX = new ();
        public Atom.PlayVfx PlayVFX = new ();
        public Atom.Broadcast Broadcast = new();
        private string guid;
        private string cachedValue;

        public void Awake()
        {
            guid = GetInstanceID() + "_destroy";//Guid.NewGuid().ToString("N");
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<DestroyOnEnum>(), this.GetType().Name);
            Broadcast.Setup(gameObject, this.GetType().Name, guid);
            PlaySFX.Setup<DestroyOn>(gameObject);
            PlayVFX.Setup<DestroyOn>(gameObject);
        }

        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.H))
        //         Export();
        // }

        public (string type, string data) Export()
        {
            DestroyOnComponent comp = new();
            
            comp.IsConditionAvailable = true;
            
            comp.IsBroadcastable = !string.IsNullOrEmpty(Broadcast.data.broadcastName);
            comp.Broadcast = string.IsNullOrEmpty(Broadcast.data.broadcastName) ? "None" : Broadcast.data.broadcastName;
            comp.BroadcastListen = string.IsNullOrEmpty(startOn.data.listenName) ? "None" : startOn.data.listenName;
            comp.broadcastTypeIndex = Broadcast.data.broadcastTypeIndex;

            comp.canPlaySFX = PlaySFX.data.canPlay;
            comp.canPlayVFX = PlayVFX.data.canPlay;

            comp.sfxName = Helper.GetSfxClipNameByIndex(PlaySFX.data.clipIndex);
            comp.vfxName = Helper.GetVfxClipNameByIndex(PlayVFX.data.clipIndex);

            comp.sfxIndex = PlaySFX.data.clipIndex;
            comp.vfxIndex = PlayVFX.data.clipIndex;

            comp.ConditionType = GetStartEvent();
            comp.ConditionData = GetStartCondition();
            comp.listenIndex = startOn.data.listenIndex;
            
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp, Formatting.Indented);
            // Debug.Log(data);
            return (type, data);
        }
        
        public string GetStartEvent(string _input = null)
        {
            int index = startOn.data.startIndex;
            string inputString = ((DestroyOnEnum)index).ToString();
            
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (Enum.TryParse(inputString, out DestroyOnEnum enumValue))
            {
                var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(enumValue);
                return eventName;
            }
            return EditorOp.Resolve<DataProvider>().GetEnumValue(DestroyOnEnum.OnClick);
        }

        
        
        public string GetStartCondition(string _input = null)
        {
            int index = startOn.data.startIndex;
            string inputString = ((DestroyOnEnum)index).ToString();
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (inputString.ToLower().Contains("listen"))
            {
                return EditorOp.Resolve<DataProvider>().GetListenString(startOn.data.listenIndex);
            }
            else
            {
                if (Enum.TryParse(inputString, out DestroyOnEnum enumValue))
                {
                    return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(enumValue);
                }
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(DestroyOnEnum.OnClick);
            }
        }
        public void Import(EntityBasedComponent cdata)
        {
            DestroyOnComponent comp = JsonConvert.DeserializeObject<DestroyOnComponent>($"{cdata.data}");
            
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(DestroyOnEnum), out object result))
            {
                startOn.data.startIndex = (int)(DestroyOnEnum)result;
            }
            
            EditorOp.Resolve<DataProvider>().UpdateToListenList(guid,comp.Broadcast);

            startOn.data.listenIndex = comp.listenIndex;
            startOn.data.startName = comp.ConditionType;
            startOn.data.listenName = comp.ConditionData;

            Broadcast.data.broadcastName = string.IsNullOrEmpty(comp.Broadcast) ? "None" : comp.Broadcast;
            Broadcast.data.broadcastTypeIndex = comp.broadcastTypeIndex;
            
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, 
                this.GetType().Name, 
                Broadcast.data.broadcastName, 
                startOn.data.listenName);
        }
    }
}
