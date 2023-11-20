using Leopotam.EcsLite;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class MeleeWeaponSystem : BaseSystem
    {
        private const string ATTACK_RESOURCE_NAME = "MeleeAttack";
        private GameObject meleeAttack;
        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            ref var entityRef = ref entity.GetComponent<MeleeWeaponComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.isKinematic = true;
            rb.useGravity = false;
            InitializeUI(in entityRef,entity);
        }

        private void InitializeUI(in MeleeWeaponComponent component, int entity)
        {
            if (meleeAttack)
            {
                return;
            }
            var resourceDB = (ResourceDB)SystemOp.Load(ResourceTag.ResourceDB);
            var itemData = resourceDB.GetItemDataForNearestName(ATTACK_RESOURCE_NAME);
            meleeAttack = RuntimeOp.Load<GameObject>(itemData.ResourcePath);
        }

        public override void OnConditionalCheck(int entity, object data)
        {  
            ref var entityRef = ref EntityAuthorOp.GetComponent<MeleeWeaponComponent>(entity);
            if (entityRef.isEquipped)
                return;
            OnDemandRun(ref entityRef, entity);
        }

        public void OnDemandRun(ref MeleeWeaponComponent component, int entity)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.RefObj.transform.position);
            }
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }


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
                          break;
                      }
                  }
              });
            LoadUI(in component, entity);
            component.isEquipped = true;
        }

        private void LoadUI(in MeleeWeaponComponent component, int entity)
        {
            var go = RuntimeOp.Resolve<View>().AttachDynamicUI(entity, meleeAttack);
            var btn = go.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            var value = component.attackAnimation;
            var obj = component.RefObj;
            btn.onClick.AddListener(() => {
                RuntimeOp.Resolve<GameData>()
              .ExecutePlayerMeleeAttack(obj);
            });
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<MeleeWeaponComponent>().End();
            var compPool = currentWorld.GetPool<MeleeWeaponComponent>();
            foreach (var entity in filter)
            {
                var component = compPool.Get(entity);
                if (component.IsExecuted)
                {
                    continue;
                }
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, component.EventContext);
            }
        }
    }
}
