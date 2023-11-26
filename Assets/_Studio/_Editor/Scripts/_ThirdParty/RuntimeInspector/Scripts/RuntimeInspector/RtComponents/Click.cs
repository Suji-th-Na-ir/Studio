using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Click")]
    public class Click : BaseBehaviour
    {
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(Click);
        public override bool CanPreview => true;
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
            PlaySFX.Setup<Click>(gameObject);
            PlayVFX.Setup<Click>(gameObject);
            broadcastData.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            var data = new ClickComponent()
            {
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.MouseAction",
                ConditionData = "OnClick",
                Listen = Listen.Always,
                FXData = GetFXData()
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<ClickComponent>(data.data);
            MapSFXAndVFXData(obj.FXData);
            broadcastData.broadcast = obj.Broadcast;
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
            var startOnName = StartOn.OnClick.ToString();
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
