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
                canPlaySFX = playSFX.data.CanPlay,
                sfxName = playSFX.data.ClipName,
                sfxIndex = playSFX.data.ClipIndex,
                canPlayVFX = playVFX.data.CanPlay,
                vfxName = playVFX.data.ClipName,
                vfxIndex = playVFX.data.ClipIndex
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
            playSFX.data.CanPlay = obj.canPlaySFX;
            playSFX.data.ClipIndex = obj.sfxIndex;
            playSFX.data.ClipName = obj.sfxName;
            playVFX.data.CanPlay = obj.canPlayVFX;
            playVFX.data.ClipIndex = obj.vfxIndex;
            playVFX.data.ClipName = obj.vfxName;
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
