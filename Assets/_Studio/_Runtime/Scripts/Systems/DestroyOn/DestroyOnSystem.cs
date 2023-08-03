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
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, (obj) =>
            {
                var go = obj == null ? null : obj as GameObject;
                OnConditionalCheck((entity, conditionType, conditionData, goRef, go));
            },
            true, (goRef, conditionData));
            if (destroyable.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(destroyable.Broadcast);
            }
        }

        public void OnConditionalCheck(object data)
        {
            var (entity, conditionType, conditionData, go, selection) = ((int, string, string, GameObject, GameObject))data;
            if (conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (selection == null || selection != go)
                {
                    return;
                }
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, null, false, (go, conditionData));
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var destroyPool = world.GetPool<DestroyOnComponent>();
            OnDemandRun(entity, in destroyPool.Get(entity));
        }

        public void OnDemandRun(int entityID, in DestroyOnComponent component)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.refObj.transform.position);
            }
            //Unsubscribe to all listeners
            //Destroy gracefully
            Object.Destroy(component.refObj);
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
            EntityAuthorOp.Degenerate(entityID);
        }
    }
}
