using UnityEngine;
using PlayShifu.Terra;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class StopTranslateSystem : BaseSystem
    {
        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            ref var entityRef = ref entity.GetComponent<StopTranslateComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<StopTranslateComponent>();
            var isTranslateFound = CheckIfRotateComponentExistsOnEntity(entity);
            if (!isTranslateFound)
            {
                Debug.Log($"Translate system not found on entity: {entity} for stop translate to act on");
                return;
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            ref var translateRef = ref entity.GetComponent<TranslateComponent>();
            if (translateRef.ConditionType.Equals("Terra.Studio.GameStart"))
            {
                translateRef.IsExecuted = true;
            }
            else if (!translateRef.isHaltedByEvent && translateRef.CanExecute)
            {
                translateRef.CanExecute = false;
                compsData.ProvideEventContext(true, translateRef.EventContext);
            }
            translateRef.isHaltedByEvent = true;
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in StopTranslateComponent component)
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
