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
                Debug.Log($"Rotate system not found on entity: {entity} for stop rotate to act on", entityRef.RefObj);
                return;
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            ref var rotateRef = ref entity.GetComponent<RotateComponent>();
            if (rotateRef.ConditionType.Equals("Terra.Studio.GameStart"))
            {
                rotateRef.IsExecuted = true;
            }
            else if (!rotateRef.isHaltedByEvent && rotateRef.CanExecute)
            {
                rotateRef.CanExecute = false;
                compsData.ProvideEventContext(true, rotateRef.EventContext);
            }
            rotateRef.isHaltedByEvent = true;
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in StopRotateComponent component)
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
