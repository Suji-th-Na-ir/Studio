using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class InstantiateStudioObjectSystem : BaseSystem
    {
        public override void Init<T>(int entity)
        {
            ref var entityRef = ref entity.GetComponent<InstantiateStudioObjectComponent>();
            entityRef.RefObj.SetActive(false);
            var context = entityRef.EventContext;
            context.additionalData = new InvokeAfterData()
            {
                rounds = entityRef.rounds,
                invokeAtStart = true
            };
            entityRef.EventContext = context;
            base.Init<T>(entity);
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<InstantiateStudioObjectComponent>();
            entityRef.CanExecute = true;
            if (entityRef.instantiateOn == InstantiateOn.EveryXSeconds)
            {
                entityRef.currentRound++;
                if (entityRef.currentRound > entityRef.rounds)
                {
                    entityRef.IsExecuted = true;
                }
            }
            if (entityRef.instantiateOn == InstantiateOn.GameStart)
            {
                entityRef.IsExecuted = true;
            }
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in InstantiateStudioObjectComponent component)
        {
            Instantiate(component);
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

        private void Instantiate(in InstantiateStudioObjectComponent component)
        {
            for (int i = 0; i < component.duplicatesToSpawn; i++)
            {
                var spawnPoint = component.RefObj.transform.position;
                if (component.spawnWhere == SpawnWhere.Random)
                {
                    var randomDelta = Random.onUnitSphere * Random.Range(1f, 3f);
                    spawnPoint += randomDelta;
                }
                var duplicate = Object.Instantiate(component.RefObj, component.RefObj.transform.parent);
                duplicate.SetActive(true);
                EntityAuthorOp.HandleComponentsGeneration(duplicate, component.componentsOnSelf);
                var refTr = duplicate.transform;
                refTr.position = spawnPoint;
                var childrenEntities = component.childrenEntities;
                if (childrenEntities == null || childrenEntities.Length == 0) continue;
                for (int j = 0; j < childrenEntities.Length; j++)
                {
                    var entityData = childrenEntities[j];
                    GameObject childGo;
                    if (refTr.childCount < j + 1)
                    {
                        entityData.shouldLoadAssetAtRuntime = true;
                        childGo = EntityAuthorOp.ProvideGameobjectForEntity(entityData);
                        childGo.transform.SetParent(refTr);
                    }
                    else
                    {
                        childGo = refTr.GetChild(j).gameObject;
                        RuntimeWrappers.ResolveTRS(childGo, null, entityData.position, entityData.rotation, entityData.scale);
                    }
                    RuntimeOp.Resolve<SceneDataHandler>().SetColliderData(childGo, entityData.metaData);
                    EntityAuthorOp.HandleEntityAndComponentsGeneration(childGo, entityData);
                }
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