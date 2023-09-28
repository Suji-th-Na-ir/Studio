using Terra.Studio;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Respawn"), AliasDrawer("Kill Player")]
    public class Respawn : BaseBehaviour
    {
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string Broadcast = null;

        protected override string ComponentName => nameof(Respawn);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            Broadcast
        };

        protected override void Awake()
        {
            base.Awake();
            PlaySFX.Setup<Respawn>(gameObject);
            PlayVFX.Setup<Respawn>(gameObject);
        }

        public override (string type, string data) Export()
        {
            RespawnComponent comp = new();
            {
                comp.IsConditionAvailable = true;
                comp.ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.OnPlayerCollide);
                comp.ConditionData = "Player";
                comp.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                comp.Broadcast = Broadcast;
                comp.canPlaySFX = PlaySFX.data.canPlay;
                comp.canPlayVFX = PlayVFX.data.canPlay;
                comp.sfxName = PlaySFX.data.clipName;
                comp.vfxName = PlayVFX.data.clipName;
                comp.sfxIndex = PlaySFX.data.clipIndex;
                comp.vfxIndex = PlayVFX.data.clipIndex;
            }
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            RespawnComponent comp = JsonConvert.DeserializeObject<RespawnComponent>(data.data);
            Broadcast = comp.Broadcast;
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            ImportVisualisation(Broadcast, null);
        }
    }
}
