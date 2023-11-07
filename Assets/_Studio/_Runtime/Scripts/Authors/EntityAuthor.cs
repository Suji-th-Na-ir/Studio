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
                CreateVisualRepresentation(virtualEntity, (x =>
                {
                    HandleEntityAndComponentGeneration(x, virtualEntity);    
                }));
                
            }

            private void CreateVisualRepresentation(VirtualEntity entity, Action<GameObject> cb)
            {
                GameObject generatedObj = null;
                if (!entity.shouldLoadAssetAtRuntime)
                {
                    cb?.Invoke(null);
                    return;
                    // return cb?.Invoke(null);
                }
                var trs = new Vector3[] { entity.position, entity.rotation, entity.scale };
                RuntimeWrappers.SpawnObject(entity.assetType, entity.assetPath, entity.primitiveType, (x)=>
                {
                    if (x != null)
                    {
                        x.name = entity.name;
                    }
                    cb?.Invoke(x);   
                },entity.uniqueName,trs);
                // generatedObj.name = entity.name;
                // return generatedObj;
            }

            private void HandleEntityAndComponentGeneration(GameObject go, VirtualEntity virtualEntity)
            {
                if (go == null && virtualEntity.shouldLoadAssetAtRuntime)
                {
                    return;
                }
                RuntimeOp.Resolve<SceneDataHandler>().SetColliderData(go, virtualEntity.metaData);
                var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                if (virtualEntity.components != null && virtualEntity.components.Length > 0)
                {
                    var entity = ecsWorld.NewEntity();
                    RuntimeOp.Resolve<EntitiesGraphics>().TrackEntityVisual(entity, go);
                    foreach (var component in virtualEntity.components)
                    {
                        ComponentAuthorOp.Generate(new ComponentGenerateData()
                        {
                            entity = entity,
                            data = component,
                            obj = go
                        });
                    }
                }
                RuntimeOp.Resolve<SceneDataHandler>().HandleChildren(go, virtualEntity.children, HandleEntityAndComponentGeneration);
            }

            public override void Degenerate(int entityID)
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
