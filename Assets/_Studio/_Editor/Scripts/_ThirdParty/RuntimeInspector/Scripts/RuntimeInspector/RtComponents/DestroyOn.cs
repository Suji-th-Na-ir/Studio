using Terra.Studio;
using PlayShifu.Terra;
using Newtonsoft.Json;

namespace RuntimeInspectorNamespace
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

    [EditorDrawComponent("Terra.Studio.DestroyOn"), AliasDrawer("Destroy Self")]
    public class DestroyOn : BaseBehaviour
    {
        [AliasDrawer("DestroyWhen")]
        public Atom.StartOn StartOn = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string Broadcast = null;

        public override string ComponentName => nameof(DestroyOn);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            Broadcast
        };
        protected override string[] ListenerRefs => new string[]
        {
            StartOn.data.listenName
        };

        protected override void Awake()
        {
            base.Awake();
            StartOn.Setup<DestroyOnEnum>(gameObject, ComponentName, OnListenerUpdated, StartOn.data.startIndex == 3);
            PlaySFX.Setup<DestroyOn>(gameObject);
            PlayVFX.Setup<DestroyOn>(gameObject);
        }

        public override (string type, string data) Export()
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
            Broadcast = comp.Broadcast;
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            string listenstring = "";
            if (StartOn.data.startIndex == 3)
            {
                listenstring = StartOn.data.listenName;
            }
            ImportVisualisation(Broadcast, listenstring);
        }
    }
}
