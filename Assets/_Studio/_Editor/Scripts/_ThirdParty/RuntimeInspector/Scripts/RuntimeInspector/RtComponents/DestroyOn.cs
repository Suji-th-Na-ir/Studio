using PlayShifu.Terra;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.DestroyOn"), AliasDrawer("Destroy Self")]
    public class DestroyOn : BaseBehaviour
    {
        public enum DestroyOnEnum
        {
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
            OnObjectCollide,
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
            OnClick,
            [EditorEnumField("Terra.Studio.Listener"), AliasDrawer("Broadcast Listened")]
            BroadcastListen
        }

        [AliasDrawer("DestroyWhen")]
        public Atom.StartOn StartOn = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();

        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(DestroyOn);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastData.broadcast
        };
        protected override string[] ListenerRefs => new string[]
        {
            StartOn.data.listenName
        };
        protected override Atom.PlaySfx[] Sfxes => new Atom.PlaySfx[]
        {
            PlaySFX
        };
        protected override Atom.PlayVfx[] Vfxes => new Atom.PlayVfx[]
        {
            PlayVFX
        };

        protected override void Awake()
        {
            base.Awake();
            StartOn.Setup<DestroyOnEnum>(gameObject, ComponentName, OnListenerUpdated, StartOn.data.startIndex == 3);
            broadcastData.Setup(gameObject, this);
            PlaySFX.Setup<DestroyOn>(gameObject);
            PlayVFX.Setup<DestroyOn>(gameObject);
        }

        public override (string type, string data) Export()
        {
            DestroyOnComponent comp = new()
            {
                FXData = GetFXData(),
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                Listen = Listen.Once
            };
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp, Formatting.Indented);
            return (type, data);
        }

        public string GetStartEvent()
        {
            int index = StartOn.data.startIndex;
            var value = (DestroyOnEnum)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }

        public string GetStartCondition()
        {
            int index = StartOn.data.startIndex;
            var value = (DestroyOnEnum)index;
            if (value.ToString().ToLower().Contains("listen"))
            {
                return StartOn.data.listenName;
            }
            else
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            }
        }

        public override void Import(EntityBasedComponent cdata)
        {
            DestroyOnComponent comp = JsonConvert.DeserializeObject<DestroyOnComponent>(cdata.data);
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(DestroyOnEnum), out object result))
            {
                var res = (DestroyOnEnum)result;
                if (res == DestroyOnEnum.OnPlayerCollide)
                {
                    if (comp.ConditionData.Equals("Player"))
                    {
                        StartOn.data.startIndex = (int)res;
                    }
                    else
                    {
                        StartOn.data.startIndex = (int)DestroyOnEnum.OnObjectCollide;
                    }
                }
                else
                {
                    StartOn.data.startIndex = (int)(DestroyOnEnum)result;
                }
                StartOn.data.startName = res.ToString();
            }
            StartOn.data.listenName = comp.ConditionData;
            broadcastData.broadcast = comp.Broadcast;
            MapSFXAndVFXData(comp.FXData);
            string listenstring = "";
            if (StartOn.data.startIndex == 3)
            {
                listenstring = StartOn.data.listenName;
            }
            ImportVisualisation(broadcastData.broadcast, listenstring);
        }

        public override BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            var properties = new Dictionary<string, object>[1];
            properties[0] = new();
            if (PlaySFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, PlaySFX.data.clipName);
            }
            if (PlayVFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, PlayVFX.data.clipName);
            }
            var broadcasts = new string[] { broadcastData.broadcast };
            var eventIndex = StartOn.data.startIndex;
            var eventEnum = (DestroyOnEnum)eventIndex;
            var eventName = eventEnum.ToString();
            var listenTo = string.Empty;
            if (eventEnum == DestroyOnEnum.BroadcastListen)
            {
                listenTo = StartOn.data.listenName;
            }
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                Properties = properties,
                Broadcast = broadcasts,
                EventName = eventName,
                Listen = listenTo
            };
            return previewData;
        }
    }
}
