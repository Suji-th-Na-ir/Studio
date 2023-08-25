using System.Linq;
using UnityEngine;

namespace Terra.Studio
{
    public class EntityAuthorOp
    {
        public static IAuthor Author => Author<EntityAuthor>.Current;

        public static void Generate()
        {
            Author.Generate();
        }

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
                var trs = new Vector3[] { entity.position, entity.rotation, entity.scale };
                GameObject generatedObj = RuntimeWrappers.SpawnObject(entity.assetType, entity.assetPath, entity.primitiveType, trs);
                return generatedObj;
            }

            private void HandleEntityAndComponentGeneration(GameObject go, VirtualEntity virtualEntity)
            {
                if (go == null)
                {
                    return;
                }
                go.name = virtualEntity.name;
                RuntimeOp.Resolve<SceneDataHandler>().SetColliderData(go, virtualEntity.metaData);
                var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                if (virtualEntity.components != null && virtualEntity.components.Length > 0)
                {
                    var entity = ecsWorld.NewEntity();
                    foreach (var component in virtualEntity.components)
                    {
                        ComponentAuthorOp.Generate((entity, component, go));
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
                ecsWorld.DelEntity(entityID);
            }
        }
    }
}
