using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Teleport"), AliasDrawer("Teleport Player")]
    public class Teleport : BaseBehaviour
    {
        [AliasDrawer("Teleport\nTo")]
        public Vector3 teleportTo;
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string broadcast = null;
        //[AliasDrawer("Do\nAlways")]
        //public bool executeMultipleTimes = true;

        public override string ComponentName => nameof(Teleport);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcast
        };

        private void Start()
        {
            if (teleportTo == Vector3.zero)
            {
                var currentPos = transform.localPosition;
                currentPos.y += 2f;
                currentPos.z += 2f;
                teleportTo = currentPos;
            }
        }

        public override (string type, string data) Export()
        {
            var data = new TeleportComponent()
            {
                teleportTo = teleportTo,
                canPlaySFX = playSFX.data.canPlay,
                sfxName = playSFX.data.clipName,
                sfxIndex = playSFX.data.clipIndex,
                canPlayVFX = playVFX.data.canPlay,
                vfxName = playVFX.data.clipName,
                vfxIndex = playVFX.data.clipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(broadcast),
                Broadcast = broadcast,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Player",
                listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<TeleportComponent>(data.data);
            teleportTo = obj.teleportTo;
            playSFX.data.canPlay = obj.canPlaySFX;
            playSFX.data.clipName = obj.sfxName;
            playSFX.data.clipIndex = obj.sfxIndex;
            playVFX.data.canPlay = obj.canPlayVFX;
            playVFX.data.clipName = obj.vfxName;
            playVFX.data.clipIndex = obj.vfxIndex;
            broadcast = obj.Broadcast;
            //executeMultipleTimes = obj.listen == Listen.Always;
            ImportVisualisation(broadcast, null);
        }
    }
}
