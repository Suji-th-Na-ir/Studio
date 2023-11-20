using Leopotam.EcsLite;
using UnityEngine;

namespace Terra.Studio
{
    public class MeleeDamageableSystem : BaseSystem
    {
        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            ref var entityRef = ref entity.GetComponent<MeleeDamageableComponent>();
            entityRef.currentHealth = entityRef.health;
        }
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<MeleeDamageableComponent>(entity);

            if (data == null)
                return;
            
            var go = data as GameObject;
            if (go)
            {
                if (go.transform.CompareTag("Damager"))
                {
                    var currentWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                    var filter = currentWorld.Filter<MeleeWeaponComponent>().End();
                    var compPool = currentWorld.GetPool<MeleeWeaponComponent>();
                    foreach (var entity1 in filter)
                    {
                        if (entity1 == entity)
                        {
                            continue;
                        }
                        var componentToCheck = compPool.Get(entity1);
                        if (go == componentToCheck.RefObj)
                        {
                            OnDemandRun(ref entityRef, componentToCheck.damage);
                            break;
                        }
                    }
                }
            }
           
            if (entityRef.currentHealth <= 0)
            {
                entityRef.IsExecuted = true;
                EntityAuthorOp.Degenerate(entity);
            }
        }

        

        public void OnDemandRun(ref MeleeDamageableComponent component, int damage)
        {
            component.currentHealth = Mathf.Clamp(component.currentHealth - damage, 0, int.MaxValue);
            if (component.currentHealth <= 0)
            {
                if (component.canPlaySFXDead)
                {
                    RuntimeWrappers.PlaySFX(component.sfxNameDead);
                }
                if (component.canPlayVFXDead)
                {
                    RuntimeWrappers.PlayVFX(component.vfxNameDead, component.RefObj.transform.position);
                }
                if (component.IsBroadcastableDead)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(component.BroadcastDead, true);
                }
            }
            else
            {
                if (component.canPlaySFXHit)
                {
                    RuntimeWrappers.PlaySFX(component.sfxNameHit);
                }
                if (component.canPlayVFXHit)
                {
                    RuntimeWrappers.PlayVFX(component.vfxNameHit, component.RefObj.transform.position);
                }
                if (component.IsBroadcastable)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
                }
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<MeleeDamageableComponent>().End();
            var compPool = currentWorld.GetPool<MeleeDamageableComponent>();
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
