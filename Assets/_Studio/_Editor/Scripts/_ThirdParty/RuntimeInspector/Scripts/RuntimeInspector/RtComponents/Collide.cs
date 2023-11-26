using Newtonsoft.Json;
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

        public StartOn startOn = StartOn.OnPlayerCollide;
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
        protected override Atom.PlaySfx[] Sfxes => new Atom.PlaySfx[]
        {
            playSFX
        };
        protected override Atom.PlayVfx[] Vfxes => new Atom.PlayVfx[]
        {
            playVFX
        };

        protected override void Awake()
        {
            base.Awake();
            playSFX.Setup<Collide>(gameObject);
            playVFX.Setup<Collide>(gameObject);
            broadcastData.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            var data = new CollideComponent()
            {
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(startOn),
                ConditionData = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(startOn),
                Listen = Listen.Always,
                FXData = GetFXData()
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<CollideComponent>(data.data);
            broadcastData.broadcast = obj.Broadcast;
            if (obj.ConditionData.Equals("Player"))
            {
                startOn = StartOn.OnPlayerCollide;
            }
            else
            {
                startOn = StartOn.OnObjectCollide;
            }
            MapSFXAndVFXData(obj.FXData);
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
