using Newtonsoft.Json;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Respawn"), AliasDrawer("Kill Player")]
    public class Respawn : BaseBehaviour
    {
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(Respawn);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastData.broadcast
        };

        protected override void Awake()
        {
            base.Awake();
            broadcastData.Setup(gameObject, this);
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
                comp.IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast);
                comp.Broadcast = broadcastData.broadcast;
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
            broadcastData.broadcast = comp.Broadcast;
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            ImportVisualisation(broadcastData.broadcast, null);
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
            var startOnName = StartOn.OnPlayerCollide.ToString();
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                EventName = startOnName,
                Properties = properties,
                Broadcast = new string[0]
            };
            return previewData;
        }
    }
}
