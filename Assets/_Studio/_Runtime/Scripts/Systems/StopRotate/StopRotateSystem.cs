using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class StopRotateSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<StopRotateComponent>();
            var isRotateFound = CheckIfRotateComponentExistsOnEntity(entity);
            if (!isRotateFound)
            {
                Debug.Log($"Rotate system not found on entity: {entity} for stop rotate to act on");
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, entityRef.EventContext);
                return;
            }
            ref var rotateRef = ref entity.GetComponent<RotateComponent>();
            //Do something!
            OnDemandRun(in entityRef, entity);
        }

        public void OnDemandRun(in StopRotateComponent component, int _)
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
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast);
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<StopRotateComponent>().End();
            var compPool = currentWorld.GetPool<StopRotateComponent>();
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

        private bool CheckIfRotateComponentExistsOnEntity(int entity)
        {
            var currentWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
            var filter = currentWorld.Filter<RotateComponent>().End();
            foreach (var otherEntity in filter)
            {
                if (otherEntity == entity)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
