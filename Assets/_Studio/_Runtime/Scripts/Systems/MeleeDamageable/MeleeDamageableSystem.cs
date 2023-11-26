using UnityEngine;

namespace Terra.Studio
{
    public class MeleeDamageableSystem : BaseSystem<MeleeDamageableComponent>
    {
        public override void Init(int entity)
        {
            base.Init(entity);
            ref var entityRef = ref entity.GetComponent<MeleeDamageableComponent>();
            entityRef.currentHealth = entityRef.health;
        }

        protected override void OnConditionalCheck(int entity, object data)
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
                DeleteEntity(entity);
            }
        }

        public void OnDemandRun(ref MeleeDamageableComponent component, int damage)
        {
            component.currentHealth = Mathf.Clamp(component.currentHealth - damage, 0, int.MaxValue);
            if (component.currentHealth > 0)
            {
                PlayFXIfExists(component, 0);
                Broadcast(component);
            }
            else
            {
                PlayFXIfExists(component, 1);
                if (component.IsBroadcastableDead)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(component.BroadcastDead);
                }
            }
        }
    }
}
