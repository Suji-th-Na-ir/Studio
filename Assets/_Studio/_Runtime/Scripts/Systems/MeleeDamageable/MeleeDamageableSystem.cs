using System.ComponentModel;
using Leopotam.EcsLite;
using PlayShifu.Terra;
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
            Debug.Log($"Broadcast Dead: {entityRef.BroadcastDead}  Health: {entityRef.health}");
        }
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<MeleeDamageableComponent>(entity);

            if (data == null)
                return;
            if(entityRef.currentHealth>0)
            {
                entityRef.currentHealth -= 1;
            }

            OnDemandRun(in entityRef, entity);
            Debug.Log($"Broadcast Dead: {entityRef.BroadcastDead}  Health: {entityRef.health}");
            if (entityRef.currentHealth <= 0)
            {
                entityRef.IsExecuted = true;
                EntityAuthorOp.Degenerate(entity);
            }
        }

        

        public void OnDemandRun(in MeleeDamageableComponent component, int entity)
        {
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
