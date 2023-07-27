using System;
using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class CollectableSystem : IAbsRunsystem, IConditionalOp
    {
        public void Init(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<CollectableComponent>().End();
            var collectablePool = currentWorld.GetPool<CollectableComponent>();
            foreach (var entity in filter)
            {
                ref var collectable = ref collectablePool.Get(entity);
                if (collectable.isRegistered)
                {
                    continue;
                }
                collectable.isRegistered = true;
                //Always look for some condition.
                //Do not have this check if condition is available at all
                var conditionType = collectable.ConditionType;
                var goRef = collectable.refObject;
                var conditionData = collectable.ConditionData;
                var compsData = Interop<RuntimeInterop>.Current.Resolve<ComponentsData>();
                compsData.ProvideEventContext(conditionType, (obj) =>
                {
                    OnConditionalCheck((entity, conditionType, goRef, conditionData, obj));
                },
                true, (goRef, conditionData));
                if (collectable.IsBroadcastable)
                {
                    Interop<RuntimeInterop>.Current.Resolve<Broadcaster>().SetBroadcastable(collectable.Broadcast);
                }
            }
        }

        public void OnConditionalCheck(object data)
        {
            var tuple = ((int entity, string conditionType, GameObject go, string conditionData, object selection))data;
            if (tuple.conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (tuple.selection == null || tuple.selection as GameObject != tuple.go)
                {
                    return;
                }
            }
            var compsData = Interop<RuntimeInterop>.Current.Resolve<ComponentsData>();
            compsData.ProvideEventContext(tuple.conditionType, null, false, (tuple.go, tuple.conditionData));
            var world = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
            var filter = world.Filter<CollectableComponent>().End();
            var collectablePool = world.GetPool<CollectableComponent>();
            OnDemandRun(tuple.entity, ref collectablePool.Get(tuple.entity));
        }

        public void OnDemandRun(int entityID, ref CollectableComponent component)
        {
            //Play SFX if any
            //Play VFX if any
            //Unsubscribe to all listeners
            //Destroy gracefully
            UnityEngine.Object.Destroy(component.refObject);
            Author<EntityAuthor>.Current.Degenerate(entityID);
            if (component.IsBroadcastable)
            {
                Interop<RuntimeInterop>.Current.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
        }
    }
}
