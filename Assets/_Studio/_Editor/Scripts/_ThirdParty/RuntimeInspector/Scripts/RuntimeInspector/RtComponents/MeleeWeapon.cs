using Newtonsoft.Json;
using PlayShifu.Terra;
using UnityEngine;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.MeleeWeapon"), AliasDrawer("Melee Weapon")]
    public class MeleeWeapon : BaseBehaviour
    {
        public enum StartEnum
        {
            [EditorEnumField("Terra.Studio.GameStart", "Start"), AliasDrawer("Game Starts")]
            GameStart,
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
            OnClick,
            [EditorEnumField("Terra.Studio.Listener"), AliasDrawer("Broadcast Listened")]
            BroadcastListen
        }

        [Header("Equipping:"), AliasDrawer("Equip When")]
        public Atom.StartOn StartOn = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public Atom.Broadcast broadcastData = new();

        [Header("Hitting:"),AliasDrawer("Animation")]
        public MeleeWeaponComponent.AttackAnimation attackAnimation;
        [Range(0, 100)]
        public int damage = 10;

        [AliasDrawer("Play SFX")]
        public Atom.PlaySfx PlaySFXAttack = new();
        [AliasDrawer("Play VFX")]
        public Atom.PlayVfx PlayVFXAttack= new();

        public override string ComponentName => nameof(MeleeWeapon);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;

        protected override void Awake()
        {
            base.Awake();
            StartOn.Setup<StartEnum>(gameObject, ComponentName, OnListenerUpdated, StartOn.data.startIndex == 2);
            PlaySFX.Setup<Click>(gameObject);
            PlayVFX.Setup<Click>(gameObject);
            broadcastData.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            MeleeWeaponComponent comp = new()
            {
                damage = this.damage,
                attackAnimation = this.attackAnimation,
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = Helper.GetSfxClipNameByIndex(PlaySFX.data.clipIndex),
                vfxName = Helper.GetVfxClipNameByIndex(PlayVFX.data.clipIndex),
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,

                canPlaySFXAttack = PlaySFXAttack.data.canPlay,
                canPlayVFXAttack = PlayVFXAttack.data.canPlay,
                sfxNameAttack = Helper.GetSfxClipNameByIndex(PlaySFXAttack.data.clipIndex),
                vfxNameAttack = Helper.GetVfxClipNameByIndex(PlayVFXAttack.data.clipIndex),
                sfxIndexAttack = PlaySFX.data.clipIndex,
                vfxIndexAttack = PlayVFX.data.clipIndex,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast
            };
            gameObject.TrySetTrigger(false, true);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp, Formatting.Indented);
            return (type, data);
        }

        public string GetStartEvent()
        {
            int index = StartOn.data.startIndex;
            var value = (StartEnum)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }

        public string GetStartCondition()
        {
            int index = StartOn.data.startIndex;
            var value = (StartEnum)index;
            if (value.ToString().ToLower().Contains("listen"))
            {
                return StartOn.data.listenName;
            }
            else
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            }
        }

        public override void Import(EntityBasedComponent cdata)
        {
            MeleeWeaponComponent comp = JsonConvert.DeserializeObject<MeleeWeaponComponent>(cdata.data);
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartEnum), out object result))
            {
                var res = (StartEnum)result;
                if (res == StartEnum.OnPlayerCollide)
                {
                    
                    if (comp.ConditionData.Equals("Player"))
                    {
                        StartOn.data.startIndex = (int)res;
                    }
                }
                else
                {
                    StartOn.data.startIndex = (int)(StartEnum)result;
                }
                StartOn.data.startName = res.ToString();
            }
            StartOn.data.listenName = comp.ConditionData;
            damage = comp.damage;
            attackAnimation = comp.attackAnimation;
            broadcastData.broadcast = comp.Broadcast;
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            PlaySFXAttack.data.canPlay = comp.canPlaySFXAttack;
            PlaySFXAttack.data.clipIndex = comp.sfxIndexAttack;
            PlaySFXAttack.data.clipName = comp.sfxNameAttack;
            PlayVFXAttack.data.canPlay = comp.canPlayVFXAttack;
            PlayVFXAttack.data.clipIndex = comp.vfxIndexAttack;
            PlayVFXAttack.data.clipName = comp.vfxNameAttack;
            string listenstring = "";
            if (StartOn.data.startIndex == 3)
            {
                listenstring = StartOn.data.listenName;
            }
            ImportVisualisation(broadcastData.broadcast, listenstring);
        }
    }
}
