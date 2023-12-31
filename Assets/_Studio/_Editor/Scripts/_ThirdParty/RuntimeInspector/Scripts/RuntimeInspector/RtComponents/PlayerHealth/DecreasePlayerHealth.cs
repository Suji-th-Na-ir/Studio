using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.DecreasePlayerHealth"), AliasDrawer("Decrease Player Health")]
    public class DecreasePlayerHealth : BaseBehaviour
    {
        public enum StartOn
        {
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
            PlayerTouches,
            [EditorEnumField("Terra.Studio.Listener"), AliasDrawer("Broadcast Listened")]
            BroadcastListen
        }

        [SerializeField] private Atom.StartOn when = new();
        [SerializeField, Range(0, 100)] private uint byPoint = 10;
        [SerializeField] private Atom.PlaySfx playSFX = new();
        [SerializeField] private Atom.PlayVfx playVFX = new();
        [SerializeField] private Atom.Broadcast broadcast = new();

        public override string ComponentName => nameof(DecreasePlayerHealth);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[] { broadcast.broadcast };
        protected override string[] ListenerRefs => new string[] { when.data.listenName };
        protected override Atom.PlaySfx[] Sfxes => new Atom.PlaySfx[] { playSFX };
        protected override Atom.PlayVfx[] Vfxes => new Atom.PlayVfx[] { playVFX };

        protected override void Awake()
        {
            base.Awake();
            when.Setup<StartOn>(gameObject, ComponentName, OnListenerUpdated, when.data.startIndex == (int)StartOn.BroadcastListen);
            playSFX.Setup<DecreasePlayerHealth>(gameObject);
            playVFX.Setup<DecreasePlayerHealth>(gameObject);
            broadcast.Setup(gameObject, this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorOp.Resolve<SceneDataHandler>().SetupPlayerHealthManager(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorOp.Resolve<SceneDataHandler>()?.SetupPlayerHealthManager(false);
        }

        public override (string type, string data) Export()
        {
            var data = new DecreasePlayerHealthComponent()
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue((StartOn)when.data.startIndex),
                ConditionData = GetConditionData(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcast.broadcast),
                Broadcast = broadcast.broadcast,
                FXData = GetFXData(),
                playerHealthModifier = byPoint,
                Listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        private string GetConditionData()
        {
            return when.data.startIndex switch
            {
                1 => when.data.listenName,
                _ => EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue((StartOn)when.data.startIndex),
            };
        }

        public override void Import(EntityBasedComponent data)
        {
            var component = JsonConvert.DeserializeObject<DecreasePlayerHealthComponent>(data.data);
            byPoint = component.playerHealthModifier;
            MapSFXAndVFXData(component.FXData);
            broadcast.broadcast = component.Broadcast;
            EditorOp.Resolve<DataProvider>().TryGetEnum(component.ConditionType, typeof(StartOn), out var result);
            var startType = (StartOn)result;
            when.data.startName = startType.ToString();
            when.data.startIndex = (int)startType;
            if (startType == StartOn.BroadcastListen)
            {
                when.data.listenName = component.ConditionData;
            }
            ImportVisualisation(broadcast.broadcast, when.data.listenName);
        }

        public override BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            var startOn = (StartOn)when.data.startIndex;
            var eventName = startOn.GetStringValue();
            var properties = new Dictionary<string, object>[1];
            properties[0] = new()
            {
                { "When", eventName },
                { "By Point", byPoint }
            };
            if (playSFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFX.data.clipName);
            }
            if (playVFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFX.data.clipName);
            }
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                Broadcast = new string[] { broadcast.broadcast },
                Properties = properties,
                EventName = eventName,
                Listen = when.data.listenName
            };
            return previewData;
        }
    }
}
