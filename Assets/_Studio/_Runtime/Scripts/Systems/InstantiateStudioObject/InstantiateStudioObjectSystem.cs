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
            var rounds = entityRef.canRepeatForver ? uint.MaxValue : entityRef.rounds;
            context.additionalData = new InvokeAfterData()
            {
                rounds = rounds,
                invokeAtStart = true
            };
            if (entityRef.instantiateOn == InstantiateOn.EveryXSeconds) entityRef.rounds--;
            entityRef.EventContext = context;
            base.Init<T>(entity);
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<InstantiateStudioObjectComponent>();
            entityRef.CanExecute = true;
            if (entityRef.instantiateOn == InstantiateOn.EveryXSeconds)
            {
                if (!entityRef.canRepeatForver)
                {
                    entityRef.currentRound++;
                    if (entityRef.currentRound > entityRef.rounds)
                    {
                        entityRef.IsExecuted = true;
                    }
                }
            }
            if (entityRef.instantiateOn == InstantiateOn.GameStart)
            {
                entityRef.IsExecuted = true;
            }
            OnDemandRun(entityRef);
        }

        public void OnDemandRun(InstantiateStudioObjectComponent component)
        {
            Instantiate(component);
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            CoroutineService.RunCoroutine(() =>
            {
                if (component.IsBroadcastable)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast);
                }
            }, CoroutineService.DelayType.WaitForFrame);
        }

        private void Instantiate(InstantiateStudioObjectComponent component)
        {
            var iterations = component.duplicatesToSpawn;
            Vector3[] points;
            if (component.spawnWhere == SpawnWhere.CurrentPoint)
            {
                points = new Vector3[iterations];
                for (int i = 0; i < iterations; i++)
                {
                    points[i] = component.RefObj.transform.position;
                }
            }
            else
            {
                points = component.GetRandomPoints(iterations);
            }
            for (int i = 0; i < component.duplicatesToSpawn; i++)
            {
                var spawnPoint = points[i];
                var duplicate = RuntimeWrappers.DuplicateGameObject(component.RefObj, component.RefObj.transform.parent, spawnPoint);
                duplicate.SetActive(true);
                var randomEntityValue = Random.Range(int.MinValue, int.MaxValue);
                RuntimeOp.Resolve<EntitiesGraphics>().TrackEntityVisual(randomEntityValue, duplicate);
                EntityAuthorOp.HandleComponentsGeneration(duplicate, component.componentsOnSelf);
                var refTr = duplicate.transform;
                var childrenEntities = component.childrenEntities;
                if (component.canPlayVFX)
                {
                    RuntimeWrappers.PlayVFX(component.vfxName, duplicate.transform.position);
                }
                if (childrenEntities == null || childrenEntities.Length == 0) continue;
                for (int j = 0; j < childrenEntities.Length; j++)
                {
                    var entityData = childrenEntities[j];
                    GameObject childGo;
                    if (refTr.childCount < j + 1)
                    {
                        entityData.shouldLoadAssetAtRuntime = true;
                        EntityAuthorOp.ProvideGameobjectForEntity(entityData, (go) =>
                        {
                            SetupChildGo(go, in entityData, component);
                        });
                    }
                    else
                    {
                        childGo = refTr.GetChild(j).gameObject;
                        RuntimeWrappers.ResolveTRS(childGo, null, entityData.position, entityData.rotation, entityData.scale);
                        SetupChildGo(childGo, in entityData, component);
                    }
                }
            }
        }

        private void SetupChildGo(GameObject childGo, in VirtualEntity entityData, InstantiateStudioObjectComponent component)
        {
            RuntimeOp.Resolve<SceneDataHandler>().SetColliderData(childGo, entityData.metaData);
            EntityAuthorOp.HandleEntityAndComponentsGeneration(childGo, entityData);
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