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

        [Header("Hitting:"), AliasDrawer("Animation")]
        public MeleeWeaponComponent.AttackAnimation attackAnimation;
        [Range(0, 100)]
        public int damage = 10;

        [AliasDrawer("Play SFX")]
        public Atom.PlaySfx PlaySFXAttack = new();
        [AliasDrawer("Play VFX")]
        public Atom.PlayVfx PlayVFXAttack = new();

        public override string ComponentName => nameof(MeleeWeapon);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override Atom.PlaySfx[] Sfxes => new Atom.PlaySfx[]
        {
            PlaySFX,
            PlaySFXAttack
        };
        protected override Atom.PlayVfx[] Vfxes => new Atom.PlayVfx[]
        {
            PlayVFX,
            PlayVFXAttack
        };

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
                damage = damage,
                attackAnimation = attackAnimation,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                FXData = GetFXData()
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
            MapSFXAndVFXData(comp.FXData);
            string listenstring = "";
            if (StartOn.data.startIndex == 3)
            {
                listenstring = StartOn.data.listenName;
            }
            ImportVisualisation(broadcastData.broadcast, listenstring);
        }
    }
}
