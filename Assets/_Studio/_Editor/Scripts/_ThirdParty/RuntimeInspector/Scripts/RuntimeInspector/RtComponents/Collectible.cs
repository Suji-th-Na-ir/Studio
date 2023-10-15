using Terra.Studio;
using Newtonsoft.Json;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
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
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string Broadcast = null;

        public override string ComponentName => nameof(Collectible);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            Broadcast
        };

        protected override void Awake()
        {
            base.Awake();
            StartOn.Setup<StartOnCollectible>(gameObject, ComponentName);
            PlaySFX.Setup<Collectible>(gameObject);
            PlayVFX.Setup<Collectible>(gameObject);
        }

        public override (string type, string data) Export()
        {
            CollectableComponent comp = new();
            {
                comp.IsConditionAvailable = true;
                comp.ConditionType = GetStartEvent();
                comp.ConditionData = GetStartCondition();
                comp.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                comp.Broadcast = Broadcast;
                comp.canPlaySFX = PlaySFX.data.canPlay;
                comp.canPlayVFX = PlayVFX.data.canPlay;
                comp.sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName;
                comp.vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName;
                comp.sfxIndex = PlaySFX.data.clipIndex;
                comp.vfxIndex = PlayVFX.data.clipIndex;
                comp.canUpdateScore = Score.score != 0;
                comp.scoreValue = Score.score;
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
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOnCollectible), out object result))
            {
                StartOn.data.startIndex = (int)(StartOnCollectible)result;
            }
            Broadcast = comp.Broadcast;
            StartOn.data.startName = comp.ConditionType;
            StartOn.data.listenName = comp.ConditionData;
            if (Score.score != 0)
            {
                EditorOp.Resolve<SceneDataHandler>()?.UpdateScoreModifiersCount(true, Score.instanceId, false);
            }
            ImportVisualisation(Broadcast, null);
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
            var broadcastArray = new string[] { Broadcast };
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
