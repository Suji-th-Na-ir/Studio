using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Checkpoint")]
    public class Checkpoint : BaseBehaviour
    {
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(Checkpoint);
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
            PlaySFX.Setup<Checkpoint>(gameObject);
            PlayVFX.Setup<Checkpoint>(gameObject);
            broadcastData.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            var component = new CheckpointComponent()
            {
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                respawnPoint = transform.position,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Player",
                Broadcast = broadcastData.broadcast,
                Listen = Listen.Always,
                FXData = GetFXData()
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            var json = data.data;
            var component = JsonConvert.DeserializeObject<CheckpointComponent>(json);
            MapSFXAndVFXData(component.FXData);
            broadcastData.broadcast = component.Broadcast;
            ImportVisualisation(broadcastData.broadcast, null);
        }
    }
}