using System.Linq;
using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class EntityAuthor : BaseAuthor
    {
        public override void Generate(object data)
        {
            var virtualEntity = (VirtualEntity)data;
            var go = RuntimeWrappers.SpawnPrimitive(virtualEntity.primitiveType, virtualEntity.position, virtualEntity.rotation, virtualEntity.scale);
            if (virtualEntity.components == null || virtualEntity.components.Length == 0)
            {
                return;
            }
            var ecsWorld = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
            var entity = ecsWorld.NewEntity();
            foreach (var component in virtualEntity.components)
            {
                Author<ComponentAuthor>.Current.Generate((entity, component, go));
            }
        }

        public override void Degenerate(int entityID)
        {
            var ecsWorld = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
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
