using System.Linq;
using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public static class EntityAuthorOp
    {
        public static IAuthor Author => Author<EntityAuthor>.Current;

        public static void Generate(object data)
        {
            Author.Generate(data);
        }

        public static void HandleComponentsGeneration(GameObject refObj, EntityBasedComponent[] components)
        {
            ((EntityAuthor)Author).HandleComponentsGeneration(refObj, components);
        }

        public static void HandleEntityAndComponentsGeneration(GameObject go, VirtualEntity virtualEntity)
        {
            ((EntityAuthor)Author).HandleEntityAndComponentGeneration(go, virtualEntity);
        }

        public static void Degenerate(int entityID)
        {
            Author.Degenerate(entityID);
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
                var go = CreateVisualRepresentation(virtualEntity);
                HandleEntityAndComponentGeneration(go, virtualEntity);
            }

            private GameObject CreateVisualRepresentation(VirtualEntity entity)
            {
                GameObject generatedObj = null;
                if (!entity.shouldLoadAssetAtRuntime)
                {
                    return generatedObj;
                }
                var trs = new Vector3[] { entity.position, entity.rotation, entity.scale };
                generatedObj = RuntimeWrappers.SpawnObject(entity.assetType, entity.assetPath, entity.primitiveType, trs);
                generatedObj.name = entity.name;
                return generatedObj;
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
                if (go == null && virtualEntity.shouldLoadAssetAtRuntime)
                {
                    return;
                }
                RuntimeOp.Resolve<SceneDataHandler>().SetColliderData(go, virtualEntity.metaData);
                if (virtualEntity.components != null && virtualEntity.components.Length > 0)
                {
                    HandleComponentsGeneration(go, virtualEntity.components);
                }
                RuntimeOp.Resolve<SceneDataHandler>().HandleChildren(go, virtualEntity.children, HandleEntityAndComponentGeneration);
            }

            public override void Degenerate(int entityID)
            {
                CoroutineService.RunCoroutine(() =>
                {
                    DestroyEntity(entityID);
                },
                CoroutineService.DelayType.WaitForFrame);
            }

            private void DestroyEntity(int entityID)
            {
                var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                var entities = new int[0];
                ecsWorld.GetAllEntities(ref entities);
                if (entities.Length == 0 || entities.Any(x => x == entityID) == false)
                {
                    Debug.Log($"Entity {entityID} not found!");
                    return;
                }
                CheckAndHandleDestroyTypes(ecsWorld, entityID);
                ecsWorld.DelEntity(entityID);
            }

            private void CheckAndHandleDestroyTypes(EcsWorld world, int toCheckEntity)
            {
                CheckAndHandleDestroyType<DestroyOnComponent>(world, toCheckEntity);
                CheckAndHandleDestroyType<CollectableComponent>(world, toCheckEntity);
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
