using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class MeleeWeaponSystem : BaseSystem<MeleeWeaponComponent>
    {
        private const string ATTACK_RESOURCE_NAME = "MeleeAttack";
        private GameObject meleeAttack;

        public override void Init(int entity)
        {
            base.Init(entity);
            ref var entityRef = ref entity.GetComponent<MeleeWeaponComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.isKinematic = true;
            rb.useGravity = false;
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (meleeAttack)
            {
                return;
            }
            var resourceDB = (ResourceDB)SystemOp.Load(ResourceTag.ResourceDB);
            var itemData = resourceDB.GetItemDataForNearestName(ATTACK_RESOURCE_NAME);
            meleeAttack = RuntimeOp.Load<GameObject>(itemData.ResourcePath);
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<MeleeWeaponComponent>(entity);
            if (entityRef.isEquipped) return;
            OnDemandRun(ref entityRef, entity);
        }

        public void OnDemandRun(ref MeleeWeaponComponent component, int entity)
        {
            PlayFXIfExists(component, 0);
            Broadcast(component);
            RuntimeOp.Resolve<GameData>()
              .SetMeleeWeaponParentTransform(component.RefObj.transform, () =>
              {
                  var currentWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                  var filter = currentWorld.Filter<MeleeWeaponComponent>().End();
                  foreach (var entity1 in filter)
                  {
                      if (entity1 == entity)
                      {
                          ref MeleeWeaponComponent componentToCheck = ref entity1.GetComponent<MeleeWeaponComponent>();
                          componentToCheck.isEquipped = false;
                          RuntimeOp.Resolve<View>().RemoveDynamicUI(nameof(MeleeWeaponComponent));
                          break;
                      }
                  }
              });
            component.isEquipped = true;
            LoadUI(in component, entity);
        }

        private void LoadUI(in MeleeWeaponComponent component, int entity)
        {
            var value = component.attackAnimation;
            var obj = component.RefObj;
            var comp = component;
            CoroutineService.RunCoroutine(() =>
            {
                var go = RuntimeOp.Resolve<View>().AttachDynamicUI(nameof(MeleeWeaponComponent), meleeAttack);
                if (!go)
                    return;
                var btn = go.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();

                btn.onClick.AddListener(() =>
                {
                    Attack(in comp, obj);
                });
            }, CoroutineService.DelayType.WaitForXFrames, 1);
        }

        private void Attack(in MeleeWeaponComponent component, GameObject obj)
        {
            RuntimeOp.Resolve<GameData>()
                 .ExecutePlayerMeleeAttack(obj);
            PlayFXIfExists(component, 1);
        }
    }
}
