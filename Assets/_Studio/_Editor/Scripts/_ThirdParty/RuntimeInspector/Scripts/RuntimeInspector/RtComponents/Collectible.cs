using Newtonsoft.Json;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Collectable")]
    public class Collectible : BaseBehaviour
    {
        public enum StartOnCollectible
        {
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
            OnClick
        }

        [AliasDrawer("CollectWhen")]
        public Atom.StartOn StartOn = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.ScoreData Score = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(Collectible);
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
            StartOn.Setup<StartOnCollectible>(gameObject, ComponentName);
            PlaySFX.Setup<Collectible>(gameObject);
            PlayVFX.Setup<Collectible>(gameObject);
            Score.Setup(gameObject, this);
            broadcastData.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            CollectableComponent comp = new();
            {
                comp.IsConditionAvailable = true;
                comp.ConditionType = GetStartEvent();
                comp.ConditionData = GetStartCondition();
                comp.IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast);
                comp.Broadcast = broadcastData.broadcast;
                comp.FXData = GetFXData();
                comp.canUpdateScore = Score.score != 0;
                comp.scoreValue = Score.score;
                comp.Listen = Listen.Once;
            }
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public string GetStartEvent()
        {
            int index = StartOn.data.startIndex;
            var value = (StartOnCollectible)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }

        public string GetStartCondition()
        {
            int index = StartOn.data.startIndex;
            var value = (StartOnCollectible)index;
            return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
        }

        public override void Import(EntityBasedComponent cdata)
        {
            CollectableComponent comp = JsonConvert.DeserializeObject<CollectableComponent>(cdata.data);
            Score.score = comp.scoreValue;
            MapSFXAndVFXData(comp.FXData);
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOnCollectible), out object result))
            {
                StartOn.data.startIndex = (int)(StartOnCollectible)result;
            }
            broadcastData.broadcast = comp.Broadcast;
            StartOn.data.startName = comp.ConditionType;
            StartOn.data.listenName = comp.ConditionData;
            if (Score.score != 0)
            {
                EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(true, Score.instanceId, false);
            }
            ImportVisualisation(broadcastData.broadcast, null);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Score.score != 0)
            {
                EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(true, Score.instanceId);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(false, Score.instanceId);
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
            properties[0].Add("Score", Score.score);
            var broadcastArray = new string[] { broadcastData.broadcast };
            var index = StartOn.data.startIndex;
            var startName = (StartOnCollectible)index;
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                Properties = properties,
                Broadcast = broadcastArray,
                EventName = startName.ToString()
            };
            return previewData;
        }
    }
}
