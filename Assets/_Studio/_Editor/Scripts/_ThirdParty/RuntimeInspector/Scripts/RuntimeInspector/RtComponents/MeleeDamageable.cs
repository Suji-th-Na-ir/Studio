using PlayShifu.Terra;
using Newtonsoft.Json;
using UnityEngine;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.MeleeDamageable"), AliasDrawer("Damageable")]
    public class MeleeDamageable : BaseBehaviour
    {
        [Range(0, 100)]
        public int Health = 10;
        [Header("When Hit"), AliasDrawer("Play SFX")]
        public Atom.PlaySfx PlaySFXHit = new();
        [AliasDrawer("Play VFX")]
        public Atom.PlayVfx PlayVFXHit = new();
        [AliasDrawer("Broadcast")]
        public Atom.Broadcast broadcastDataHit = new();
        [Header("When Dead"), AliasDrawer("Play SFX")]
        public Atom.PlaySfx PlaySFXDead = new();
        [AliasDrawer("Play VFX")]
        public Atom.PlayVfx PlayVFXDead = new();
        [AliasDrawer("Broadcast")]
        public Atom.Broadcast broadcastDataDead = new();

        public override string ComponentName => nameof(MeleeDamageable);

        public override bool CanPreview => false;

        protected override bool CanBroadcast => true;

        protected override bool CanListen => false;

        protected override string[] BroadcasterRefs => new string[]
        {

            broadcastDataHit.broadcast,
            broadcastDataDead.broadcast
        };

        protected override void Awake()
        {
            base.Awake();

            broadcastDataHit.Setup(gameObject, this);
            PlaySFXHit.Setup<MeleeDamageable>(gameObject);
            PlayVFXHit.Setup<MeleeDamageable>(gameObject);

            broadcastDataDead.Setup(gameObject, this);
            PlaySFXDead.Setup<MeleeDamageable>(gameObject);
            PlayVFXDead.Setup<MeleeDamageable>(gameObject);
        }

        public override (string type, string data) Export()
        {
            MeleeDamageableComponent comp = new()
            {
                IsConditionAvailable = true,
                health = Health,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Any",
                canPlaySFXHit = PlaySFXHit.data.canPlay,
                canPlayVFXHit = PlayVFXHit.data.canPlay,
                sfxNameHit = Helper.GetSfxClipNameByIndex(PlaySFXHit.data.clipIndex),
                vfxNameHit = Helper.GetVfxClipNameByIndex(PlayVFXHit.data.clipIndex),
                sfxIndexHit = PlaySFXHit.data.clipIndex,
                vfxIndexHit = PlayVFXHit.data.clipIndex,
                IsBroadcastable = !string.IsNullOrEmpty(broadcastDataHit.broadcast),
                Broadcast = broadcastDataHit.broadcast,

                canPlaySFXDead = PlaySFXDead.data.canPlay,
                canPlayVFXDead = PlayVFXDead.data.canPlay,
                sfxNameDead = Helper.GetSfxClipNameByIndex(PlaySFXDead.data.clipIndex),
                vfxNameDead = Helper.GetVfxClipNameByIndex(PlayVFXDead.data.clipIndex),
                sfxIndexDead = PlaySFXDead.data.clipIndex,
                vfxIndexDead = PlayVFXDead.data.clipIndex,
                IsBroadcastableDead = !string.IsNullOrEmpty(broadcastDataDead.broadcast),
                BroadcastDead = broadcastDataDead.broadcast,
            };
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp, Formatting.Indented);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            MeleeDamageableComponent comp = JsonConvert.DeserializeObject<MeleeDamageableComponent>(data.data);
            Health = comp.health;
            PlaySFXHit.data.canPlay = comp.canPlaySFXHit;
            PlayVFXHit.data.canPlay = comp.canPlayVFXHit;
            PlaySFXHit.data.clipIndex = comp.sfxIndexHit;
            PlayVFXHit.data.clipIndex = comp.vfxIndexHit;
            PlaySFXHit.data.clipName = comp.sfxNameHit;
            PlayVFXHit.data.clipName = comp.vfxNameHit;
            PlaySFXDead.data.canPlay = comp.canPlaySFXDead;
            PlayVFXDead.data.canPlay = comp.canPlayVFXDead;
            PlaySFXDead.data.clipIndex = comp.sfxIndexDead;
            PlayVFXDead.data.clipIndex = comp.vfxIndexDead;
            PlaySFXDead.data.clipName = comp.sfxNameDead;
            PlayVFXDead.data.clipName = comp.vfxNameDead;
            broadcastDataHit.broadcast = comp.Broadcast;
            broadcastDataDead.broadcast = comp.BroadcastDead;
            ImportVisualisation(broadcastDataHit.broadcast, string.Empty);
            ImportVisualisation(broadcastDataDead.broadcast, string.Empty);
        }
    }
}
