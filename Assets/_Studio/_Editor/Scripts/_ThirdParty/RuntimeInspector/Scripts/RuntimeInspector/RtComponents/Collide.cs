using Newtonsoft.Json;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Collide")]
    public class Collide : BaseBehaviour
    {
        public enum StartOn
        {
            [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
            OnObjectCollide
        }

        public Atom.StartOn startOn = new();
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(Collide);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastData.broadcast
        };

        //[AliasDrawer("Do\nAlways")]
        //public bool executeMultipleTimes = true;

        protected override void Awake()
        {
            base.Awake();
            startOn.Setup<StartOn>(gameObject, ComponentName);
            playSFX.Setup<Collide>(gameObject);
            playVFX.Setup<Collide>(gameObject);
            broadcastData.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            var start = Helper.GetEnumValueByIndex<StartOn>(startOn.data.startIndex);
            var data = new CollideComponent()
            {
                canPlaySFX = playSFX.data.canPlay,
                sfxName = playSFX.data.clipName,
                sfxIndex = playSFX.data.clipIndex,
                canPlayVFX = playVFX.data.canPlay,
                vfxName = playVFX.data.clipName,
                vfxIndex = playVFX.data.clipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(start),
                ConditionData = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(start),
                startIndex = startOn.data.startIndex,
                startName = startOn.data.startName,
                listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<CollideComponent>(data.data);
            playSFX.data.canPlay = obj.canPlaySFX;
            playSFX.data.clipName = obj.sfxName;
            playSFX.data.clipIndex = obj.sfxIndex;
            playVFX.data.canPlay = obj.canPlayVFX;
            playVFX.data.clipName = obj.vfxName;
            playVFX.data.clipIndex = obj.vfxIndex;
            broadcastData.broadcast = obj.Broadcast;
            startOn.data.startIndex = obj.startIndex;
            startOn.data.startName = obj.startName;
            //executeMultipleTimes = obj.listen == Listen.Always;
            ImportVisualisation(broadcastData.broadcast, null);
        }

        public override BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            var properties = new Dictionary<string, object>[1];
            properties[0] = new();
            if (playSFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFX.data.clipName);
            }
            if (playVFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFX.data.clipName);
            }
            var startOnName = Studio.StartOn.OnPlayerCollide.ToString();
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                EventName = startOnName,
                Properties = properties,
                Broadcast = new string[] { broadcastData.broadcast }
            };
            return previewData;
        }
    }
}
