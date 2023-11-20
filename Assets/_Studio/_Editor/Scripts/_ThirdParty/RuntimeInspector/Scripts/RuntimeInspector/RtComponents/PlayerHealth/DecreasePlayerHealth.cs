using UnityEngine;
using Newtonsoft.Json;

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
        [SerializeField, Range(0, 100)] private uint byPoint;
        [SerializeField] private Atom.PlaySfx playSFX = new();
        [SerializeField] private Atom.PlayVfx playVFX = new();
        [SerializeField] private Atom.Broadcast broadcast = new();

        public override string ComponentName => nameof(DecreasePlayerHealth);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[] { broadcast.broadcast };
        protected override string[] ListenerRefs => new string[] { when.data.listenName };

        protected override void Awake()
        {
            base.Awake();
            when.Setup<StartOn>(gameObject, ComponentName, OnListenerUpdated, when.data.startIndex == (int)StartOn.BroadcastListen);
            playSFX.Setup<DecreasePlayerHealth>(gameObject);
            playVFX.Setup<DecreasePlayerHealth>(gameObject);
            broadcast.Setup(gameObject, this);
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
                canPlaySFX = playSFX.data.canPlay,
                sfxIndex = playSFX.data.clipIndex,
                sfxName = playSFX.data.clipName,
                canPlayVFX = playVFX.data.canPlay,
                vfxIndex = playVFX.data.clipIndex,
                vfxName = playVFX.data.clipName,
                playerHealthModifier = byPoint
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
            playSFX.data.canPlay = component.canPlaySFX;
            playSFX.data.clipIndex = component.sfxIndex;
            playSFX.data.clipName = component.sfxName;
            playVFX.data.canPlay = component.canPlayVFX;
            playVFX.data.clipIndex = component.vfxIndex;
            playVFX.data.clipName = component.vfxName;
            broadcast.broadcast = component.Broadcast;
            EditorOp.Resolve<DataProvider>().TryGetEnum(component.ConditionType, typeof(StartOn), out var result);
            var startType = (StartOn)result;
            when.data.startName = startType.ToString();
            when.data.startIndex = (int)startType;
            if (startType == StartOn.BroadcastListen)
            {
                when.data.listenName = component.ConditionData;
            }
        }
    }
}
