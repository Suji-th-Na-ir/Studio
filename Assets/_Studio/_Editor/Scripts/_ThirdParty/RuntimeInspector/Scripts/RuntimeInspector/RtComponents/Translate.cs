using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Translate"), AliasDrawer("Move")]
    public class Translate : MonoBehaviour, IComponent
    {
        [AliasDrawer("MoveWhen")]
        public Atom.StartOn startOn = new();
        public Atom.Translate Type = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();

        private string guid;
        private void Awake()
        {
            guid = GetInstanceID() + "_translate";
            Type.Setup(guid, gameObject, GetType().Name);
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOn>(), Helper.GetEnumWithAliasNames<StartOn>(), this.GetType().Name, startOn.data.startIndex == 4);
            PlaySFX.Setup<Translate>(gameObject);
            PlayVFX.Setup<Translate>(gameObject);
        }

        public (string type, string data) Export()
        {
            TranslateComponent comp = new TranslateComponent
            {
                translateType = (TranslateType)Type.data.translateType,
                speed = Type.data.speed,
                pauseFor = Type.data.pauseFor,
                repeatFor = Type.data.repeat,
                targetPosition = Type.data.moveBy,
                startPosition = transform.position,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                broadcastAt = Type.data.broadcastAt,
                IsBroadcastable = !string.IsNullOrEmpty(Type.data.broadcast),
                Broadcast = Type.data.broadcast,
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,
                listen = Listen.Always,
            };

            ModifyDataAsPerGiven(ref comp);
            gameObject.TrySetTrigger(false, true);
            string type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp, Formatting.Indented);
            return (type, data);
        }

        public string GetStartEvent()
        {
            int index = startOn.data.startIndex;
            var value = (StartOn)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }


        public string GetStartCondition()
        {
            int index = startOn.data.startIndex;
            var value = (StartOn)index;
            string inputString = value.ToString();
            if (inputString.ToLower().Contains("listen"))
            {
                return startOn.data.listenName;
            }
            var data = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            return data;
        }

        public void Import(EntityBasedComponent cdata)
        {
            TranslateComponent comp = JsonConvert.DeserializeObject<TranslateComponent>(cdata.data);
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            Type.data.translateType = (int)comp.translateType;
            Type.data.speed = comp.speed;
            Type.data.pauseFor = comp.pauseFor;
            Type.data.moveBy = comp.targetPosition;
            Type.data.repeat = comp.repeatFor;
            Type.data.broadcast = comp.Broadcast;
            Type.data.broadcastAt = comp.broadcastAt;
            Type.data.listenTo = comp.ConditionData;
            Type.data.listen = comp.listen;
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                var res = (StartOn)result;
                if (res == StartOn.OnPlayerCollide)
                {
                    if (comp.ConditionData.Equals("Player"))
                    {
                        startOn.data.startIndex = (int)res;
                    }
                    else
                    {
                        startOn.data.startIndex = (int)StartOn.OnObjectCollide;
                    }
                }
                else
                {
                    startOn.data.startIndex = (int)(StartOn)result;
                }
                startOn.data.startName = res.ToString();
            }
            startOn.data.listenName = comp.ConditionData;
            var listenString = "";
            if (startOn.data.startIndex == 3)
                listenString = Type.data.listenTo;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, Type.data.broadcast, listenString);
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
