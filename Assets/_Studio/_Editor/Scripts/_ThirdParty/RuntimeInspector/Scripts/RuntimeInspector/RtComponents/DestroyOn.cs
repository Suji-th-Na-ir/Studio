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
        public string Broadcast = null;
        private string guid;
        private string cachedValue;

        public void Awake()
        {
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<DestroyOnEnum>());
            PlaySFX.Setup(gameObject);
            PlayVFX.Setup(gameObject);
            guid = Guid.NewGuid().ToString("N");
        }

        public void Update()
        {
            if (Broadcast == cachedValue)
            {
                return;
            }
            cachedValue = Broadcast;
            EditorOp.Resolve<DataProvider>().UpdateListenToTypes(guid, Broadcast,gameObject);
            if (Input.GetKeyDown(KeyCode.H)) Export();
        }

        public (string type, string data) Export()
        {
            DestroyOnComponent destroyOn = new();
            
            destroyOn.IsConditionAvailable = true;
            destroyOn.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
            destroyOn.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;
            destroyOn.BroadcastListen = string.IsNullOrEmpty(startOn.data.listenName) ? null : startOn.data.listenName;

            destroyOn.canPlaySFX = PlaySFX.data.canPlay;
            destroyOn.canPlayVFX = PlayVFX.data.canPlay;

            destroyOn.sfxName = Helper.GetSfxClipNameByIndex(PlaySFX.data.clipIndex);
            destroyOn.vfxName = Helper.GetVfxClipNameByIndex(PlayVFX.data.clipIndex);

            destroyOn.sfxIndex = PlaySFX.data.clipIndex;
            destroyOn.vfxIndex = PlayVFX.data.clipIndex;

            destroyOn.ConditionType = GetStartEvent();
            destroyOn.ConditionData = GetStartCondition();
            destroyOn.listenIndex = startOn.data.listenIndex;
            
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(destroyOn, Formatting.Indented);
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
            DestroyOnComponent cc = JsonConvert.DeserializeObject<DestroyOnComponent>($"{cdata.data}");
            
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(DestroyOnEnum), out object result))
            {
                startOn.data.startIndex = (int)(DestroyOnEnum)result;
            }
            
            if (cc.ConditionType.ToLower().Contains("listen"))
            {
                EditorOp.Resolve<DataProvider>().AddToListenList(guid,cc.ConditionData);
            }
            startOn.data.listenIndex = cc.listenIndex;
            
            startOn.data.startName = cc.ConditionType;
            startOn.data.listenName = cc.ConditionData;

            Broadcast = cc.Broadcast;
            
            PlaySFX.data.canPlay = cc.canPlaySFX;
            PlaySFX.data.clipIndex = cc.sfxIndex;
            PlaySFX.data.clipName = cc.sfxName;
            PlayVFX.data.canPlay = cc.canPlayVFX;
            PlayVFX.data.clipIndex = cc.vfxIndex;
            PlayVFX.data.clipName = cc.vfxName;
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
