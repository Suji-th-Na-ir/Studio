using UnityEngine;
using Leopotam.EcsLite;
using System.Collections;
using System.Collections.Generic;

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
    }
}
