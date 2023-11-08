using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class InstantiateStudioObjectSystem : BaseSystem
    {
        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            var entityRef = EntityAuthorOp.GetComponent<InstantiateStudioObjectComponent>(entity);
            entityRef.RefObj.SetActive(false);
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<InstantiateStudioObjectComponent>(entity);
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in InstantiateStudioObjectComponent component)
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
            Instantiate(component);
        }

        private void Instantiate(in InstantiateStudioObjectComponent component)
        {
            var duplicate = UnityEngine.Object.Instantiate(component.RefObj);
            duplicate.SetActive(true);
            EntityAuthorOp.HandleComponentsGeneration(duplicate, component.componentsOnSelf.components);
            var refTr = duplicate.transform;
            var childrenEntities = component.childrenEntities;
            for (int i = 0; i < refTr.childCount; i++)
            {
                var entityData = childrenEntities[i];
                var childGO = refTr.GetChild(i).gameObject;
                RuntimeWrappers.ResolveTRS(childGO, null, entityData.position, entityData.rotation, entityData.scale);
                EntityAuthorOp.HandleEntityAndComponentsGeneration(refTr.GetChild(i).gameObject, entityData);
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<InstantiateStudioObjectComponent>().End();
            var compPool = currentWorld.GetPool<InstantiateStudioObjectComponent>();
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