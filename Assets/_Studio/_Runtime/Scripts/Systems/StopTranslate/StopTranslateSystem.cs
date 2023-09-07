using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class StopTranslateSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<StopTranslateComponent>(entity);
            if (entityRef.ConditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (data == null)
                {
                    return;
                }
                var selection = (GameObject)data;
                if (selection != entityRef.RefObj)
                {
                    return;
                }
            }
            var isRotateFound = CheckIfRotateComponentExistsOnEntity(entity);
            if (!isRotateFound)
            {
                Debug.Log($"Translate system not found on entity: {entity} for stop translate to act on");
                return;
            }
            ref var rotateRef = ref entity.GetComponent<TranslateComponent>();
            if (!rotateRef.CanExecute)
            {
                Debug.Log($"Translate system is not running on entity: {entity} for stop translate to act on");
                return;
            }
            rotateRef.isHaltedByEvent = true;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(false, entityRef.EventContext);
            OnDemandRun(in entityRef, entity);
        }

        public void OnDemandRun(in StopTranslateComponent component, int _)
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
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<StopTranslateComponent>().End();
            var compPool = currentWorld.GetPool<StopTranslateComponent>();
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
            var filter = currentWorld.Filter<TranslateComponent>().End();
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
