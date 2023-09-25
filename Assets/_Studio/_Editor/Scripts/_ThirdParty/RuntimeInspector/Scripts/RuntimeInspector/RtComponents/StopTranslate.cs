using System;
using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.StopTranslate")]
    public class StopTranslate : MonoBehaviour, IComponent
    {
        public enum StartOn
        {
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
            OnClick,
            [EditorEnumField("Terra.Studio.Listener")]
            BroadcastListen
        }

        public Atom.StartOn startOn = new();
        public string broadcast = null;
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();

        private void Awake()
        {
            startOn.Setup(gameObject,
                Helper.GetEnumValuesAsStrings<StartOn>(),
                typeof(StopTranslate).Name,startOn.data.startIndex==1);
            playSFX.Setup<StopTranslate>(gameObject);
            playVFX.Setup<StopTranslate>(gameObject);
        }

        public (string type, string data) Export()
        {
            var data = new StopTranslateComponent
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(GetEnum(startOn.data.startName)),
                ConditionData = GetConditionValue(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                startIndex = startOn.data.startIndex,
                canPlaySFX = playSFX.data.canPlay,
                sfxName = playSFX.data.clipName,
                sfxIndex = playSFX.data.clipIndex,
                canPlayVFX = playVFX.data.canPlay,
                vfxName = playVFX.data.clipName,
                vfxIndex = playVFX.data.clipIndex
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        private StartOn GetEnum(string data)
        {
            if (Enum.TryParse(data, out StartOn startOn))
            {
                return startOn;
            }
            return StartOn.OnClick;
        }

        private string GetConditionValue()
        {
            if (string.IsNullOrEmpty(startOn.data.startName) ||
                startOn.data.startName.Equals(StartOn.OnClick))
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOn.OnClick);
            }
            else
            {
                return startOn.data.listenName;
            }
        }

        public void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<StopTranslateComponent>(data.data);
            startOn.data.startName = GetStart(obj).ToString();
            startOn.data.listenName = GetListenValues(obj);
            startOn.data.startIndex = obj.startIndex;
            broadcast = obj.Broadcast;
            playSFX.data.canPlay = obj.canPlaySFX;
            playSFX.data.clipIndex = obj.sfxIndex;
            playSFX.data.clipName = obj.sfxName;
            playVFX.data.canPlay = obj.canPlayVFX;
            playVFX.data.clipIndex = obj.vfxIndex;
            playVFX.data.clipName = obj.vfxName;
            var listenString = "";
            if (startOn.data.startIndex == 1)
                listenString = startOn.data.listenName;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, broadcast, listenString);
        }

        private StartOn GetStart(StopTranslateComponent comp)
        {
            if (comp.ConditionType.Equals(EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.OnClick)))
            {
                return StartOn.OnClick;
            }
            return StartOn.BroadcastListen;
        }

        private string GetListenValues(StopTranslateComponent comp)
        {
            if (comp.ConditionType.Equals(EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.BroadcastListen)))
            {
                return comp.ConditionData;
            }
            return null;
        }
    }
}
