using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public enum DestroyOnEnum
    {
        [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
        OnPlayerCollide,
        [EditorEnumField("Terra.Studio.TriggerAction", "Other")]
        OnObjectCollide,
        [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
        OnClick,
        [EditorEnumField("Terra.Studio.Listener")]
        BroadcastListen
    }

    [EditorDrawComponent("Terra.Studio.DestroyOn")]
    public class DestroyOn : MonoBehaviour, IComponent
    {
        public Atom.StartOn startOn = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public string Broadcast = null;

        public void Awake()
        {
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<DestroyOnEnum>(), GetType().Name, startOn.data.startIndex == 2);
            PlaySFX.Setup<DestroyOn>(gameObject);
            PlayVFX.Setup<DestroyOn>(gameObject);
        }

        public (string type, string data) Export()
        {
            DestroyOnComponent comp = new()
            {
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = Helper.GetSfxClipNameByIndex(PlaySFX.data.clipIndex),
                vfxName = Helper.GetVfxClipNameByIndex(PlayVFX.data.clipIndex),
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                IsBroadcastable = !string.IsNullOrEmpty(Broadcast),
                Broadcast = Broadcast
            };
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp, Formatting.Indented);
            return (type, data);
        }

        public string GetStartEvent()
        {
            int index = startOn.data.startIndex;
            var value = (DestroyOnEnum)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }

        public string GetStartCondition()
        {
            int index = startOn.data.startIndex;
            var value = (DestroyOnEnum)index;
            if (value.ToString().ToLower().Contains("listen"))
            {
                return startOn.data.listenName;
            }
            else
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            }
        }

        public void Import(EntityBasedComponent cdata)
        {
            DestroyOnComponent comp = JsonConvert.DeserializeObject<DestroyOnComponent>(cdata.data);
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(DestroyOnEnum), out object result))
            {
                var res = (DestroyOnEnum)result;
                if (res == DestroyOnEnum.OnPlayerCollide)
                {
                    if (comp.ConditionData.Equals("Player"))
                    {
                        startOn.data.startIndex = (int)res;
                    }
                    else
                    {
                        startOn.data.startIndex = (int)DestroyOnEnum.OnObjectCollide;
                    }
                }
                else
                {
                    startOn.data.startIndex = (int)(DestroyOnEnum)result;
                }
                startOn.data.startName = res.ToString();
            }
            startOn.data.listenName = comp.ConditionData;
            Broadcast = comp.Broadcast;
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            string listenstring = "";
            if (startOn.data.startIndex == 2)
                listenstring = startOn.data.listenName;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Broadcast, listenstring);
        }
    }
}
