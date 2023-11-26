using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Push")]
    public class Push : BaseBehaviour
    {
        public float resistance = 0;
        [AliasDrawer("Reset\nButton")]
        public bool showResetButton = true;
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(Push);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
           broadcastData.broadcast
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
            broadcastData.Setup(gameObject, this);
            PlaySFX.Setup<DestroyOn>(gameObject);
            PlayVFX.Setup<DestroyOn>(gameObject);
        }

        public override (string type, string data) Export()
        {
            var component = new PushComponent()
            {
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Any",
                mass = resistance,
                showResetButton = showResetButton,
                FXData = GetFXData(),
                Listen = Listen.Always,
                Broadcast = broadcastData.broadcast,
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast)
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<PushComponent>(data.data);
            resistance = comp.mass;
            showResetButton = comp.showResetButton;
            MapSFXAndVFXData(comp.FXData);
            broadcastData.broadcast = comp.Broadcast;
            ImportVisualisation(broadcastData.broadcast, null);
        }
    }
}
