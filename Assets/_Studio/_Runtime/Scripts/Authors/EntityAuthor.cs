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
                var go = RuntimeWrappers.SpawnGameObject(virtualEntity.assetPath, virtualEntity.position, virtualEntity.rotation, virtualEntity.scale);
                if (virtualEntity.components == null || virtualEntity.components.Length == 0)
                {
                    return;
                }
                var ecsWorld = RuntimeOp.Resolve<RuntimeSystem>().World;
                var entity = ecsWorld.NewEntity();
                foreach (var component in virtualEntity.components)
                {
                    ComponentAuthorOp.Generate((entity, component, go));
                }
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
                //Delete all backlogs
                //Destroy gameobject by having a bank of gos
                ecsWorld.DelEntity(entityID);
            }
        }
    }
}
