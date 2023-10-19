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
                canPlaySFX = PlaySFX.data.canPlay,
                sfxName = PlaySFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                canPlayVFX = PlayVFX.data.canPlay,
                vfxName = PlayVFX.data.clipName,
                vfxIndex = PlayVFX.data.clipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                respawnPoint = transform.position,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Player",
                Broadcast = broadcastData.broadcast
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            var json = data.data;
            var component = JsonConvert.DeserializeObject<CheckpointComponent>(json);
            PlaySFX.data.canPlay = component.canPlaySFX;
            PlaySFX.data.clipName = component.sfxName;
            PlaySFX.data.clipIndex = component.sfxIndex;
            PlayVFX.data.canPlay = component.canPlayVFX;
            PlayVFX.data.clipName = component.vfxName;
            PlayVFX.data.clipIndex = component.vfxIndex;
            broadcastData.broadcast = component.Broadcast;
            ImportVisualisation(broadcastData.broadcast, null);
        }
    }
}
