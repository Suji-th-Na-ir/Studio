using System;
using System.Linq;
using UnityEngine;
using Leopotam.EcsLite;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public static class EntityAuthorOp
    {
        public static IAuthor Author => Author<EntityAuthor>.Current;

        public static void Generate(object data)
        {
            Author.Generate(data);
        }

        public static void ProvideGameobjectForEntity(VirtualEntity entity, Action<GameObject> onSpawned)
        {
            ((EntityAuthor)Author).CreateVisualRepresentation(entity, onSpawned);
        }

        public static void HandleComponentsGeneration(GameObject refObj, EntityBasedComponent[] components)
        {
            ((EntityAuthor)Author).HandleComponentsGeneration(refObj, components);
        }

        public static void HandleEntityAndComponentsGeneration(GameObject go, VirtualEntity virtualEntity)
        {
            ((EntityAuthor)Author).HandleEntityAndComponentGeneration(go, virtualEntity);
        }

        public static void Degenerate<T>(int entityID) where T : struct, IBaseComponent
        {
            ((EntityAuthor)Author).Degenerate<T>(entityID);
        }

        public static void Flush()
        {
            Author<EntityAuthor>.Flush();
        }

        public static ref T GetComponent<T>(this int entity) where T : struct, IBaseComponent
        {
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var pool = world.GetPool<T>();
            ref var component = ref pool.Get(entity);
            return ref component;
        }

        private class EntityAuthor : BaseAuthor
        {
            public override void Generate(object data)
            {
                var virtualEntity = (VirtualEntity)data;
                CreateVisualRepresentation(virtualEntity, (x =>
                {
                    HandleEntityAndComponentGeneration(x, virtualEntity);
                }));

            }

            public void CreateVisualRepresentation(VirtualEntity entity, Action<GameObject> cb)
            {
                if (!entity.shouldLoadAssetAtRuntime)
                {
                    cb?.Invoke(null);
                    return;
                }
                var trs = new Vector3[] { entity.position, entity.rotation, entity.scale };
                RuntimeWrappers.SpawnObject(entity.assetType, entity.assetPath, entity.primitiveType, (x) =>
                {
                    if (x != null)
                    {
                        x.name = entity.name;
                    }
                    for (int i = 0; i < x.transform.childCount; i++)
                    {
                        x.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Default");
                    }
                    cb?.Invoke(x);
                }, entity.uniqueName, trs);
            }

            public void HandleComponentsGeneration(GameObject refObj, EntityBasedComponent[] components)
            {
                if (components == null || components.Length == 0) return;
                var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                var entity = ecsWorld.NewEntity();
                foreach (var component in components)
                {
                    ComponentAuthorOp.Generate(new ComponentGenerateData()
                    {
                        entity = entity,
                        data = component,
                        obj = refObj
                    });
                }
                RuntimeOp.Resolve<EntitiesGraphics>().TrackEntityVisual(entity, refObj);
            }

            public void HandleEntityAndComponentGeneration(GameObject go, VirtualEntity virtualEntity)
            {
                if (go == null && virtualEntity.shouldLoadAssetAtRuntime) return;
                RuntimeOp.Resolve<SceneDataHandler>().SetColliderData(go, virtualEntity.metaData);
                HandleComponentsGeneration(go, virtualEntity.components);
                RuntimeOp.Resolve<SceneDataHandler>().HandleChildren(go, virtualEntity.children, HandleComponentsGeneration);
            }

            public override void Degenerate<T>(int entityID)
            {
                CoroutineService.RunCoroutine(() =>
                {
                    DestroyEntity<T>(entityID);
                },
                CoroutineService.DelayType.WaitForFrame);
            }

            private void DestroyEntity<T>(int entityID) where T : struct, IBaseComponent
            {
                var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                var entities = new int[0];
                ecsWorld.GetAllEntities(ref entities);
                if (entities.Length == 0 || entities.Any(x => x == entityID) == false)
                {
                    Debug.Log($"Entity {entityID} not found!");
                    return;
                }
                CheckAndHandleDestroyType<T>(ecsWorld, entityID);
                ecsWorld.DelEntity(entityID);
                if (entities.Length == 1)
                {
                    RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance<BaseSystem<T>, T>();
                }
            }

            private void CheckAndHandleDestroyType<T>(EcsWorld world, int toCheckEntity) where T : struct, IBaseComponent
            {
                var filter = world.Filter<T>().End();
                foreach (var entity in filter)
                {
                    if (entity == toCheckEntity)
                    {
                        var component = entity.GetComponent<T>();
                        if (component.RefObj)
                        {
                            Object.Destroy(component.RefObj);
                        }
                        return;
                    }
                }
            }
        }
    }
}
