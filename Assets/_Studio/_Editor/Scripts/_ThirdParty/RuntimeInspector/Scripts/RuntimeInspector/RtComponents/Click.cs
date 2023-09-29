using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Click")]
    public class Click : BaseBehaviour
    {
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string broadcast = null;
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        //public bool executeMultipleTimes = true;

        public override string ComponentName => nameof(Click);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcast
        };

        protected override void Awake()
        {
            base.Awake();
            PlaySFX.Setup<Click>(gameObject);
            PlayVFX.Setup<Click>(gameObject);
        }

        public override (string type, string data) Export()
        {
            var data = new ClickComponent()
            {
                canPlaySFX = PlaySFX.data.canPlay,
                sfxName = PlaySFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                canPlayVFX = PlayVFX.data.canPlay,
                vfxName = PlayVFX.data.clipName,
                vfxIndex = PlayVFX.data.clipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.MouseAction",
                ConditionData = "OnClick",
                listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<ClickComponent>(data.data);
            PlaySFX.data.canPlay = obj.canPlaySFX;
            PlaySFX.data.clipName = obj.sfxName;
            PlaySFX.data.clipIndex = obj.sfxIndex;
            PlayVFX.data.canPlay = obj.canPlayVFX;
            PlayVFX.data.clipName = obj.vfxName;
            PlayVFX.data.clipIndex = obj.vfxIndex;
            broadcast = obj.Broadcast;
            //executeMultipleTimes = obj.listen == Listen.Always;
            ImportVisualisation(broadcast, null);
        }
    }
}
