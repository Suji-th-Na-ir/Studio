using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class CollectableSystem : IAbsRunsystem, IConditionalOp
    {
        public void Init(EcsWorld currentWorld, int entity)
        {
            var filter = currentWorld.Filter<CollectableComponent>().End();
            var collectablePool = currentWorld.GetPool<CollectableComponent>();
            ref var collectable = ref collectablePool.Get(entity);
            if (collectable.isRegistered)
            {
                return;
            }
            collectable.isRegistered = true;
            var conditionType = collectable.ConditionType;
            var goRef = collectable.refObject;
            var conditionData = collectable.ConditionData;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, (obj) =>
            {
                OnConditionalCheck((entity, conditionType, goRef, conditionData, obj));
            },
            true, (goRef, conditionData));
            if (collectable.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(collectable.Broadcast);
            }
        }

        public void OnConditionalCheck(object data)
        {
            var (entity, conditionType, go, conditionData, selection) = ((int, string, GameObject, string, object))data;
            if (conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (selection == null || selection as GameObject != go)
                {
                    return;
                }
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, null, false, (go, conditionData));
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var collectablePool = world.GetPool<CollectableComponent>();
            OnDemandRun(entity, in collectablePool.Get(entity));
        }

        public void OnDemandRun(int entityID, in CollectableComponent component)
        {
            //Unsubscribe to all listeners
            //Destroy gracefully
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.refObject.transform.position);
            }
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
            if (component.canUpdateScore)
            {
                RuntimeWrappers.AddScore(component.scoreValue);
            }
            Object.Destroy(component.refObject);
            EntityAuthorOp.Degenerate(entityID);
        }
    }
}
