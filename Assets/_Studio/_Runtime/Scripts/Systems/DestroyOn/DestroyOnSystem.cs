using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class DestroyOnSystem : IAbsRunsystem, IConditionalOp
    {
        public void Init(EcsWorld currentWorld, int entity)
        {
            var filter = currentWorld.Filter<DestroyOnComponent>().End();
            var pool = currentWorld.GetPool<DestroyOnComponent>();
            ref var destroyable = ref pool.Get(entity);
            if (destroyable.isRegistered)
            {
                return;
            }
            destroyable.isRegistered = true;
            var conditionType = destroyable.ConditionType;
            var conditionData = destroyable.ConditionData;
            var goRef = destroyable.refObj;
            var compsData = Interop<RuntimeInterop>.Current.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, (obj) =>
            {
                var go = obj == null ? null : obj as GameObject;
                OnConditionalCheck((entity, conditionType, conditionData, goRef, go));
            },
            true, (goRef, conditionData));
            if (destroyable.IsBroadcastable)
            {
                Interop<RuntimeInterop>.Current.Resolve<Broadcaster>().SetBroadcastable(destroyable.Broadcast);
            }
        }

        public void OnConditionalCheck(object data)
        {
            var tuple = ((int entity, string conditionType, string conditionData, GameObject go, GameObject selection))data;
            if (tuple.conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (tuple.selection == null || tuple.selection != tuple.go)
                {
                    return;
                }
            }
            var compsData = Interop<RuntimeInterop>.Current.Resolve<ComponentsData>();
            compsData.ProvideEventContext(tuple.conditionType, null, false, (tuple.go, tuple.conditionData));
            var world = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
            var filter = world.Filter<DestroyOnComponent>().End();
            var destroyPool = world.GetPool<DestroyOnComponent>();
            OnDemandRun(tuple.entity, ref destroyPool.Get(tuple.entity));
        }

        public void OnDemandRun(int entityID, ref DestroyOnComponent component)
        {
            //Play SFX if any
            //Play VFX if any
            //Unsubscribe to all listeners
            //Destroy gracefully
            Debug.Log($"Destroying and broadcasting: {component.Broadcast}");
            UnityEngine.Object.Destroy(component.refObj);
            Author<EntityAuthor>.Current.Degenerate(entityID);
            if (component.IsBroadcastable)
            {
                Interop<RuntimeInterop>.Current.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
        }
    }
}
